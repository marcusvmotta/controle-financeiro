namespace FinanceControl.Application.Common.Exceptions;

// Exceções de negócio (TRD 01-arquitetura, "Tratamento de erros").
// O GlobalExceptionHandler da Api converte cada uma no status HTTP correspondente, no formato ProblemDetails.

/// <summary>404: recurso não existe ou pertence a outro usuário.</summary>
public sealed class NotFoundException(string message) : Exception(message);

/// <summary>409: conflito de estado (ex.: e-mail já cadastrado).</summary>
public sealed class ConflictException(string message) : Exception(message);

/// <summary>422: regra de negócio violada.</summary>
public sealed class BusinessRuleException(string message) : Exception(message);

/// <summary>401: credenciais ou token inválidos.</summary>
public sealed class UnauthorizedException(string message) : Exception(message);

/// <summary>403: autenticado, mas sem permissão para esta ação (ex.: usuário demo trocando a senha).</summary>
public sealed class ForbiddenException(string message) : Exception(message);
