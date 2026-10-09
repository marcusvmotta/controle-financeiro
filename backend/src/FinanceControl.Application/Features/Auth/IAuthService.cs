namespace FinanceControl.Application.Features.Auth;

/// <summary>
/// Casos de uso de autenticação. A interface fica na Application; a implementação fica na
/// Infrastructure, porque depende do ASP.NET Core Identity (ADR-004).
/// </summary>
public interface IAuthService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    /// <summary>Troca um refresh token válido por um novo par de tokens (rotação).</summary>
    Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken);

    /// <summary>Revoga o refresh token. Não falha se o token for inválido.</summary>
    Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken);

    Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken);
}
