namespace FinanceControl.Application.Features.Auth;

// DTOs (Data Transfer Objects): o formato dos dados que entram e saem da API.
// Records são imutáveis e comparados por valor, ideais para DTOs.

public sealed record RegisterRequest(string Name, string Email, string Password);

public sealed record LoginRequest(string Email, string Password);

public sealed record UserResponse(Guid Id, string Name, string Email, bool IsDemo);

/// <summary>Corpo da resposta de login, cadastro e refresh. O refresh token NÃO vai aqui: vai no cookie.</summary>
public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, UserResponse User);

/// <summary>Resultado interno do serviço: a resposta pública + o refresh token, que o controller grava no cookie.</summary>
public sealed record AuthResult(AuthResponse Response, string RefreshToken, DateTimeOffset RefreshTokenExpiresAt);

public sealed record AuthConfigResponse(bool DemoEnabled);
