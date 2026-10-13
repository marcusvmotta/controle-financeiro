namespace FinanceControl.Domain.Entities;

/// <summary>
/// Refresh token de longa duração (TRD 04-seguranca).
/// Guardamos só o hash: se o banco vazar, os tokens não podem ser usados.
/// </summary>
public sealed class RefreshToken
{
    // Construtor sem parâmetros exigido pelo EF Core para materializar a entidade a partir do banco.
    private RefreshToken()
    {
        TokenHash = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }

    /// <summary>Todas as rotações de um mesmo login compartilham a família. Usada para revogar tudo em caso de reuso.</summary>
    public Guid FamilyId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsRevoked => RevokedAt is not null;

    public static RefreshToken Issue(Guid userId, string tokenHash, Guid familyId, DateTimeOffset now, TimeSpan lifetime)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tokenHash);
        if (lifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(lifetime), "A validade do token precisa ser positiva.");
        }

        return new RefreshToken
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            TokenHash = tokenHash,
            FamilyId = familyId,
            CreatedAt = now,
            ExpiresAt = now.Add(lifetime),
        };
    }

    public bool IsExpired(DateTimeOffset now) => now >= ExpiresAt;

    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    /// <summary>Revoga o token. Revogar de novo não muda nada (operação idempotente).</summary>
    public void Revoke(DateTimeOffset now, Guid? replacedByTokenId = null)
    {
        if (IsRevoked)
        {
            return;
        }

        RevokedAt = now;
        ReplacedByTokenId = replacedByTokenId;
    }
}
