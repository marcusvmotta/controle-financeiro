using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.Api.Infrastructure;

/// <summary>
/// Erros que acontecem ANTES do FluentValidation, quando o JSON nem consegue ser convertido
/// (ex.: "type": "Bitcoin" num campo enum, ou "initialBalance": "abc").
/// Sem isto, o ASP.NET responde em inglês, com chaves como "$.type" e o nome interno das classes.
/// </summary>
public static class InvalidRequestResponseFactory
{
    public static IActionResult Create(ActionContext context)
    {
        var errors = context.ModelState
            .Where(entry => entry.Value is { Errors.Count: > 0 })
            .ToDictionary(
                entry => ToFieldName(entry.Key),
                _ => new[] { "Valor inválido para este campo." });

        if (errors.Count == 0)
        {
            errors[string.Empty] = ["O corpo da requisição é inválido."];
        }

        var problem = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos são inválidos",
        };
        problem.Extensions["traceId"] = System.Diagnostics.Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problem) { ContentTypes = { "application/problem+json" } };
    }

    /// <summary>"$.type" → "type"; "$.initialBalance" → "initialBalance"; "request" (corpo inteiro) → "".</summary>
    private static string ToFieldName(string key)
    {
        var name = key.StartsWith("$.", StringComparison.Ordinal) ? key[2..] : key;
        return name is "$" or "request" ? string.Empty : JsonNamingPolicy.CamelCase.ConvertName(name);
    }
}
