using FinanceControl.Application.Features.Auth;
using FinanceControl.Infrastructure.Identity;
using FinanceControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceControl.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra os serviços da camada de infraestrutura: banco, Identity, tokens e health checks.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            // Falhar cedo, com mensagem clara, é melhor do que um erro confuso na primeira consulta.
            throw new InvalidOperationException(
                "A connection string 'Default' não foi configurada. " +
                "Defina ConnectionStrings__Default (variável de ambiente) ou ConnectionStrings:Default (appsettings).");
        }

        services.AddDbContext<AppDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        // ---- Identity (ADR-004): só o núcleo, sem cookies de login nem telas prontas ----
        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                // Regras de senha (TRD 04-seguranca). "Uma letra" é checada no FluentValidation,
                // porque o Identity só sabe exigir minúscula/maiúscula separadamente.
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                // Bloqueio: 5 senhas erradas seguidas → 15 minutos sem conseguir logar.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddEntityFrameworkStores<AppDbContext>();

        // ---- Tokens ----
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<JwtTokenGenerator>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

        return services;
    }

    /// <summary>Aplica as migrations pendentes (TRD 02-modelo-de-dados, "Migrations").</summary>
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
    }
}
