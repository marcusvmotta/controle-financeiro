namespace FinanceControl.Api.Infrastructure;

/// <summary>Configuração do cookie do refresh token (seção "Auth"). ADR-005.</summary>
public sealed class AuthCookieOptions
{
    public const string SectionName = "Auth";
    public const string RefreshTokenCookieName = "refresh_token";

    /// <summary>
    /// Flag Secure: o navegador só envia o cookie por HTTPS.
    /// Navegadores tratam http://localhost como seguro, então funciona também no desenvolvimento local.
    /// </summary>
    public bool SecureCookies { get; init; } = true;
}
