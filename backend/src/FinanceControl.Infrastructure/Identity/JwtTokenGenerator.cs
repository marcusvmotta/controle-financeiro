using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace FinanceControl.Infrastructure.Identity;

/// <summary>Gera access tokens (JWT) e refresh tokens (aleatórios). TRD 04-seguranca.</summary>
public sealed class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
{
    private readonly JwtOptions _options = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public (string Token, DateTimeOffset ExpiresAt) CreateAccessToken(AppUser user)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            IssuedAt = now.UtcDateTime,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            // "Claims" são as informações dentro do token. Qualquer um pode LER um JWT (é só Base64),
            // então nunca coloque dados sensíveis aqui. A assinatura só garante que ninguém ALTEROU.
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            ]),
            SigningCredentials = new SigningCredentials(CreateSigningKey(_options.Key), SecurityAlgorithms.HmacSha256),
        };

        return (_handler.CreateToken(descriptor), expiresAt);
    }

    /// <summary>64 bytes aleatórios criptograficamente seguros, em Base64Url (seguro para cookies e URLs).</summary>
    public static string CreateRefreshToken() => Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(64));

    /// <summary>SHA-256 do token, em hexadecimal. É isso que vai para o banco.</summary>
    public static string HashRefreshToken(string refreshToken) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));

    public static SymmetricSecurityKey CreateSigningKey(string key) => new(Encoding.UTF8.GetBytes(key));
}
