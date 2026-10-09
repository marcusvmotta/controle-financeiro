using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FinanceControl.Application.Features.Auth;

namespace FinanceControl.IntegrationTests.Infrastructure;

/// <summary>Atalhos para os testes: cadastrar usuário, ler o cookie do refresh token, montar requisições.</summary>
public static class AuthTestHelpers
{
    public const string DefaultPassword = "senha1234";
    private const string CookieName = "refresh_token";

    public static string UniqueEmail() => $"user-{Guid.NewGuid():N}@test.com";

    public static async Task<(AuthResponse Body, string RefreshToken)> RegisterAsync(
        this HttpClient client, string? email = null, string password = DefaultPassword)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("Ana Souza", email ?? UniqueEmail(), password));
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        return (body!, GetRefreshTokenCookie(response)!);
    }

    /// <summary>POST com o cookie do refresh token e o header anti-CSRF, como o frontend faz.</summary>
    public static Task<HttpResponseMessage> PostWithRefreshCookieAsync(
        this HttpClient client, string url, string? refreshToken, bool includeCsrfHeader = true)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, url);
        if (refreshToken is not null)
        {
            request.Headers.Add("Cookie", $"{CookieName}={refreshToken}");
        }

        if (includeCsrfHeader)
        {
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
        }

        return client.SendAsync(request);
    }

    public static Task<HttpResponseMessage> GetWithTokenAsync(this HttpClient client, string url, string accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client.SendAsync(request);
    }

    /// <summary>Linha completa do Set-Cookie do refresh token (com path, httponly...).</summary>
    public static string? GetRefreshTokenSetCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith($"{CookieName}=", StringComparison.Ordinal))
            : null;

    /// <summary>Só o valor do refresh token, extraído do Set-Cookie.</summary>
    public static string? GetRefreshTokenCookie(HttpResponseMessage response)
    {
        var header = GetRefreshTokenSetCookie(response);
        return header?[(CookieName.Length + 1)..].Split(';')[0];
    }
}
