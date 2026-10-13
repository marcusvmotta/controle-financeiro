using FinanceControl.Api.Infrastructure;
using FinanceControl.Application;
using FinanceControl.Application.Abstractions;
using FinanceControl.Infrastructure;
using FinanceControl.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. Registro de serviços (injeção de dependência)
// ---------------------------------------------------------------------------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers(options =>
{
    // Sem isto, o ASP.NET valida sozinho campos string não-nulos, com mensagens em inglês.
    // Desligamos para que toda a validação fique no FluentValidation, em português.
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

// Erros no formato ProblemDetails, sempre com o traceId para cruzar com os logs (TRD 03-api).
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
    context.ProblemDetails.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// TimeProvider: abstração do .NET para "que horas são". Nos testes, dá para trocar por um relógio fixo.
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.Configure<AuthCookieOptions>(builder.Configuration.GetSection(AuthCookieOptions.SectionName));

// ---- Autenticação: valida o JWT enviado no header Authorization: Bearer <token> ----
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((bearer, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        bearer.MapInboundClaims = false; // mantém os nomes padrão do JWT ("sub", "email") em vez dos longos do .NET
        bearer.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = JwtTokenGenerator.CreateSigningKey(jwt.Key),
            ClockSkew = TimeSpan.FromSeconds(30),
            NameClaimType = JwtRegisteredClaimNames.Name,
        };
    });

// ---- Autorização: "seguro por padrão". Toda rota exige login, exceto as marcadas com [AllowAnonymous] ----
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

builder.Services.AddAppRateLimiting(builder.Configuration);

// A API roda atrás do Nginx (ADR-010). Estes headers dizem qual era o IP e o esquema (http/https) originais.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // O proxy roda numa rede Docker interna, então confiamos em qualquer origem.
    // Isso só é seguro porque a API não fica exposta diretamente na internet.
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (app.Configuration.GetValue("Database:MigrateOnStartup", defaultValue: false))
{
    await app.Services.MigrateDatabaseAsync();
}

// ---------------------------------------------------------------------------
// 2. Pipeline de middlewares (cada requisição passa por eles, nesta ordem)
// ---------------------------------------------------------------------------
app.UseForwardedHeaders();
app.UseExceptionHandler();      // captura exceções e responde com ProblemDetails
app.UseStatusCodePages();       // respostas de erro sem corpo (ex.: 401 do JWT) também viram ProblemDetails

if (app.Configuration.GetValue("Swagger:Enabled", defaultValue: app.Environment.IsDevelopment()))
{
    app.MapOpenApi().AllowAnonymous();   // documento OpenAPI em /openapi/v1.json
    app.UseSwaggerUI(options =>          // tela interativa em /swagger
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FinanceControl API");
        options.DocumentTitle = "FinanceControl API";
    });
}

app.UseAuthentication();   // quem é você? (lê o JWT)
app.UseAuthorization();    // você pode acessar isto? (aplica as políticas)
app.UseRateLimiter();

app.MapControllers();

// Liveness: o processo está de pé? Não verifica dependências.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
}).AllowAnonymous();

// Readiness: está pronto para receber tráfego? Verifica o banco (checks com a tag "ready").
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
}).AllowAnonymous();

await app.RunAsync();
