using FinanceControl.Api.Infrastructure;
using FinanceControl.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// 1. Registro de serviços (injeção de dependência)
// ---------------------------------------------------------------------------
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// TimeProvider: abstração do .NET para "que horas são". Nos testes, dá para trocar por um relógio fixo.
builder.Services.AddSingleton(TimeProvider.System);

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

// ---------------------------------------------------------------------------
// 2. Pipeline de middlewares (cada requisição passa por eles, nesta ordem)
// ---------------------------------------------------------------------------
app.UseForwardedHeaders();

if (app.Configuration.GetValue("Swagger:Enabled", defaultValue: app.Environment.IsDevelopment()))
{
    app.MapOpenApi();                    // documento OpenAPI em /openapi/v1.json
    app.UseSwaggerUI(options =>          // tela interativa em /swagger
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FinanceControl API");
        options.DocumentTitle = "FinanceControl API";
    });
}

app.MapControllers();

// Liveness: o processo está de pé? Não verifica dependências.
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false,
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});

// Readiness: está pronto para receber tráfego? Verifica o banco (checks com a tag "ready").
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteAsync,
});

app.Run();
