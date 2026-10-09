using FinanceControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceControl.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registra os serviços da camada de infraestrutura (banco, health checks e, nos próximos marcos, Identity e Hangfire).
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

        services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

        return services;
    }
}
