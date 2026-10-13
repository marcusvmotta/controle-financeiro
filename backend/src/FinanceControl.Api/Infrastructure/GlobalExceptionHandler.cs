using System.Text.Json;
using FinanceControl.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.Api.Infrastructure;

/// <summary>
/// Converte exceções em respostas ProblemDetails (RFC 9457), com o status HTTP certo (TRD 01-arquitetura).
/// Os controllers e services não precisam de try/catch: basta lançar a exceção de negócio adequada.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var problem = exception switch
        {
            ValidationException validation => CreateValidationProblem(validation),
            NotFoundException e => Problem(StatusCodes.Status404NotFound, "Recurso não encontrado", e.Message),
            ConflictException e => Problem(StatusCodes.Status409Conflict, "Conflito", e.Message),
            BusinessRuleException e => Problem(StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada", e.Message),
            UnauthorizedException e => Problem(StatusCodes.Status401Unauthorized, "Não autenticado", e.Message),
            ForbiddenException e => Problem(StatusCodes.Status403Forbidden, "Acesso negado", e.Message),
            _ => null,
        };

        if (problem is null)
        {
            // Erro inesperado: o detalhe vai só para o log; o cliente recebe uma mensagem genérica.
            logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}",
                httpContext.Request.Method, httpContext.Request.Path);
            problem = Problem(StatusCodes.Status500InternalServerError, "Erro inesperado",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.");
        }

        httpContext.Response.StatusCode = problem.Status!.Value;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static ProblemDetails Problem(int status, string title, string detail) =>
        new() { Status = status, Title = title, Detail = detail };

    private static ValidationProblemDetails CreateValidationProblem(ValidationException exception)
    {
        // Agrupa as mensagens por campo, com o nome em camelCase ("email"), igual ao JSON da requisição.
        var errors = exception.Errors
            .GroupBy(error => JsonNamingPolicy.CamelCase.ConvertName(error.PropertyName))
            .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

        return new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais campos são inválidos",
        };
    }
}
