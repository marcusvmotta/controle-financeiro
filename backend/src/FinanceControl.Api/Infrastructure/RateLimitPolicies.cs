using System.Threading.RateLimiting;

namespace FinanceControl.Api.Infrastructure;

/// <summary>Limites de requisições por IP (TRD 04-seguranca, "Proteções da API").</summary>
public static class RateLimitPolicies
{
    public const string Auth = "auth";

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var authPermitLimit = configuration.GetValue("RateLimiting:AuthPermitLimit", defaultValue: 10);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // "Janela fixa": no máximo N requisições por minuto, por IP. Passou disso, 429 até o minuto virar.
            options.AddPolicy(Auth, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = authPermitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                }));
        });

        return services;
    }
}
