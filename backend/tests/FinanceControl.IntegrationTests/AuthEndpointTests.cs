using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using FinanceControl.Application.Features.Auth;
using FinanceControl.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FinanceControl.IntegrationTests;

/// <summary>
/// Testes de ponta a ponta da autenticação contra um PostgreSQL real.
/// Cobrem os testes obrigatórios 3 e 4 do TRD (08-testes) e os critérios de aceite do RF-01.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class AuthEndpointTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateApiClient();

    // ---------------- Cadastro ----------------

    [Fact]
    public async Task Register_ValidData_Returns201WithAccessTokenAndSecureRefreshCookie()
    {
        var email = AuthTestHelpers.UniqueEmail();

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Ana Souza", email, "senha1234"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.AccessToken));
        Assert.Equal(email, body.User.Email);
        Assert.Equal("Ana Souza", body.User.Name);

        // O refresh token vem só no cookie, com as proteções do ADR-005.
        var setCookie = AuthTestHelpers.GetRefreshTokenSetCookie(response);
        Assert.NotNull(setCookie);
        Assert.Contains("httponly", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("secure", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", setCookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("path=/api/auth", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_DuplicateEmailWithDifferentCase_Returns409()
    {
        var email = AuthTestHelpers.UniqueEmail();
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Outra Pessoa", email.ToUpperInvariant(), "outrasenha1"));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("curta1")]          // menos de 8 caracteres
    [InlineData("semnumeros")]      // sem número
    [InlineData("12345678")]        // sem letra
    public async Task Register_WeakPassword_Returns400WithPasswordError(string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Ana Souza", AuthTestHelpers.UniqueEmail(), password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("password"));
    }

    // ---------------- Login ----------------

    [Fact]
    public async Task Login_ValidCredentials_Returns200()
    {
        var email = AuthTestHelpers.UniqueEmail();
        await _client.RegisterAsync(email);

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, AuthTestHelpers.DefaultPassword));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(AuthTestHelpers.GetRefreshTokenCookie(response));
    }

    [Fact]
    public async Task Login_WrongPasswordOrUnknownEmail_Returns401WithSameMessage()
    {
        var email = AuthTestHelpers.UniqueEmail();
        await _client.RegisterAsync(email);

        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "senhaerrada1"));
        var unknownEmail = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest(AuthTestHelpers.UniqueEmail(), "senhaerrada1"));

        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
        // Mesma mensagem nos dois casos: não revela se o e-mail existe.
        var first = await wrongPassword.Content.ReadFromJsonAsync<ProblemDetails>();
        var second = await unknownEmail.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(first!.Detail, second!.Detail);
    }

    [Fact]
    public async Task Login_AfterFiveWrongPasswords_AccountIsLockedEvenWithCorrectPassword()
    {
        var email = AuthTestHelpers.UniqueEmail();
        await _client.RegisterAsync(email);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, "senhaerrada1"));
        }

        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, AuthTestHelpers.DefaultPassword));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------- Rotas protegidas (teste obrigatório 4) ----------------

    [Fact]
    public async Task Me_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync("/api/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsCurrentUser()
    {
        var email = AuthTestHelpers.UniqueEmail();
        var (auth, _) = await _client.RegisterAsync(email);

        var response = await _client.GetWithTokenAsync("/api/me", auth.AccessToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.Equal(email, user!.Email);
    }

    [Fact]
    public async Task Me_WithExpiredToken_Returns401()
    {
        var (auth, _) = await _client.RegisterAsync();
        var expiredToken = CreateToken(auth.User.Id, expiresAt: DateTime.UtcNow.AddMinutes(-5), key: ApiFactory.JwtKey);

        var response = await _client.GetWithTokenAsync("/api/me", expiredToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithTokenSignedByAnotherKey_Returns401()
    {
        var (auth, _) = await _client.RegisterAsync();
        var forgedToken = CreateToken(auth.User.Id, expiresAt: DateTime.UtcNow.AddMinutes(10),
            key: "a-completely-different-key-0123456789abcdefgh");

        var response = await _client.GetWithTokenAsync("/api/me", forgedToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------------- Refresh token (teste obrigatório 3) ----------------

    [Fact]
    public async Task Refresh_ValidToken_ReturnsNewAccessTokenAndRotatesCookie()
    {
        var (_, refreshToken) = await _client.RegisterAsync();

        var response = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", refreshToken);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var newRefreshToken = AuthTestHelpers.GetRefreshTokenCookie(response);
        Assert.NotNull(newRefreshToken);
        Assert.NotEqual(refreshToken, newRefreshToken);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        var me = await _client.GetWithTokenAsync("/api/me", body!.AccessToken);
        Assert.Equal(HttpStatusCode.OK, me.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReusingRotatedToken_RevokesWholeFamily()
    {
        var (_, firstToken) = await _client.RegisterAsync();
        var rotated = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", firstToken);
        var secondToken = AuthTestHelpers.GetRefreshTokenCookie(rotated);

        // Alguém (um atacante?) reusa o primeiro token, que já foi trocado.
        var reuse = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", firstToken);
        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);

        // Consequência: até o token mais novo, legítimo, foi revogado.
        var legit = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", secondToken);
        Assert.Equal(HttpStatusCode.Unauthorized, legit.StatusCode);
    }

    [Fact]
    public async Task Refresh_WithoutCsrfHeader_Returns400()
    {
        var (_, refreshToken) = await _client.RegisterAsync();

        var response = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", refreshToken, includeCsrfHeader: false);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401AndClearsCookie()
    {
        var response = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", "token-que-nao-existe");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var setCookie = AuthTestHelpers.GetRefreshTokenSetCookie(response);
        Assert.NotNull(setCookie);
        Assert.Contains("expires=Thu, 01 Jan 1970", setCookie, StringComparison.OrdinalIgnoreCase);
    }

    // ---------------- Logout ----------------

    [Fact]
    public async Task Logout_RevokesRefreshToken()
    {
        var (_, refreshToken) = await _client.RegisterAsync();

        var logout = await _client.PostWithRefreshCookieAsync("/api/auth/logout", refreshToken);
        var refresh = await _client.PostWithRefreshCookieAsync("/api/auth/refresh", refreshToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    // ---------------- Config ----------------

    [Fact]
    public async Task Config_IsPublicAndReportsDemoDisabledByDefault()
    {
        var config = await _client.GetFromJsonAsync<AuthConfigResponse>("/api/auth/config");

        Assert.NotNull(config);
        Assert.False(config.DemoEnabled);
    }

    private static string CreateToken(Guid userId, DateTime expiresAt, string key) =>
        new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = ApiFactory.JwtIssuer,
            Audience = ApiFactory.JwtIssuer,
            IssuedAt = expiresAt.AddMinutes(-15),
            NotBefore = expiresAt.AddMinutes(-15),
            Expires = expiresAt,
            Subject = new ClaimsIdentity([new Claim(JwtRegisteredClaimNames.Sub, userId.ToString())]),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256),
        });
}
