using FinanceControl.Api.Infrastructure;
using FinanceControl.Application.Common.Exceptions;
using FinanceControl.Application.Features.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace FinanceControl.Api.Controllers;

/// <summary>
/// Cadastro, login, renovação e logout (TRD modulos/01-autenticacao.md).
/// O controller é "fino": valida a entrada, chama o service e cuida do cookie e do status HTTP.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Auth)]
public sealed class AuthController(
    IAuthService authService,
    IOptions<AuthCookieOptions> cookieOptions,
    IConfiguration configuration) : ControllerBase
{
    private const string CsrfHeaderName = "X-Requested-With";
    private const string CsrfHeaderValue = "XMLHttpRequest";

    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        RegisterRequest request, IValidator<RegisterRequest> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await authService.RegisterAsync(request, cancellationToken);

        SetRefreshTokenCookie(result);
        return Created("/api/me", result.Response);
    }

    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request, IValidator<LoginRequest> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await authService.LoginAsync(request, cancellationToken);

        SetRefreshTokenCookie(result);
        return Ok(result.Response);
    }

    /// <summary>Lê o refresh token do cookie e devolve um novo access token (e um novo cookie).</summary>
    [HttpPost("refresh")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!HasCsrfHeader())
        {
            return CsrfProblem();
        }

        var refreshToken = Request.Cookies[AuthCookieOptions.RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Não autenticado",
                detail: "Sessão inválida ou expirada. Faça login novamente.");
        }

        try
        {
            var result = await authService.RefreshAsync(refreshToken, cancellationToken);
            SetRefreshTokenCookie(result);
            return Ok(result.Response);
        }
        catch (UnauthorizedException e)
        {
            // Token inválido: apaga o cookie para o navegador não ficar reenviando.
            // A resposta é montada aqui (e não relançando a exceção) porque o middleware de exceções
            // limpa os headers da resposta, o que apagaria também este Set-Cookie.
            DeleteRefreshTokenCookie();
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Não autenticado", detail: e.Message);
        }
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        if (!HasCsrfHeader())
        {
            return CsrfProblem();
        }

        await authService.LogoutAsync(Request.Cookies[AuthCookieOptions.RefreshTokenCookieName], cancellationToken);
        DeleteRefreshTokenCookie();
        return NoContent();
    }

    /// <summary>Configurações públicas que o frontend precisa antes do login.</summary>
    [HttpGet("config")]
    [DisableRateLimiting]
    public ActionResult<AuthConfigResponse> Config() =>
        Ok(new AuthConfigResponse(configuration.GetValue("Demo:Enabled", defaultValue: false)));

    // ---- Cookie do refresh token (ADR-005) ----

    private void SetRefreshTokenCookie(AuthResult result) =>
        Response.Cookies.Append(AuthCookieOptions.RefreshTokenCookieName, result.RefreshToken, CreateCookieOptions(result.RefreshTokenExpiresAt));

    private void DeleteRefreshTokenCookie() =>
        Response.Cookies.Delete(AuthCookieOptions.RefreshTokenCookieName, CreateCookieOptions(expires: null));

    private CookieOptions CreateCookieOptions(DateTimeOffset? expires) => new()
    {
        HttpOnly = true,                         // JavaScript não consegue ler (proteção contra XSS)
        Secure = cookieOptions.Value.SecureCookies,
        SameSite = SameSiteMode.Strict,          // não é enviado em requisições vindas de outros sites (CSRF)
        Path = "/api/auth",                      // só vai para as rotas de autenticação
        Expires = expires,
        IsEssential = true,
    };

    // Defesa extra contra CSRF: um formulário de outro site não consegue enviar headers customizados.
    private bool HasCsrfHeader() =>
        string.Equals(Request.Headers[CsrfHeaderName], CsrfHeaderValue, StringComparison.OrdinalIgnoreCase);

    private ObjectResult CsrfProblem() =>
        Problem(statusCode: StatusCodes.Status400BadRequest, title: "Requisição inválida",
            detail: $"O header {CsrfHeaderName}: {CsrfHeaderValue} é obrigatório.");
}
