using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FinanceControl.Api.Infrastructure;

/// <summary>
/// Escreve o resultado dos health checks em JSON, sem expor detalhes internos (TRD 07-observabilidade).
/// </summary>
public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.ToDictionary(
                entry => entry.Key,
                entry => entry.Value.Status.ToString()),
        };

        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
