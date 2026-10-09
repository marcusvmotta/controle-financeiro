using FinanceControl.Domain.Entities;

namespace FinanceControl.UnitTests.Domain;

public sealed class RefreshTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 9, 12, 0, 0, TimeSpan.Zero);

    private static RefreshToken NewToken() =>
        RefreshToken.Issue(Guid.NewGuid(), "hash", Guid.NewGuid(), Now, TimeSpan.FromDays(7));

    [Fact]
    public void Issue_SetsExpirationFromLifetime()
    {
        var token = NewToken();

        Assert.Equal(Now.AddDays(7), token.ExpiresAt);
        Assert.True(token.IsActive(Now));
    }

    [Fact]
    public void IsActive_AfterExpiration_ReturnsFalse()
    {
        var token = NewToken();

        Assert.False(token.IsActive(Now.AddDays(7)));
        Assert.True(token.IsExpired(Now.AddDays(7)));
    }

    [Fact]
    public void Revoke_MarksAsRevokedAndRecordsReplacement()
    {
        var token = NewToken();
        var replacementId = Guid.NewGuid();

        token.Revoke(Now.AddMinutes(1), replacementId);

        Assert.True(token.IsRevoked);
        Assert.False(token.IsActive(Now.AddMinutes(2)));
        Assert.Equal(replacementId, token.ReplacedByTokenId);
    }

    [Fact]
    public void Revoke_Twice_KeepsFirstRevocation()
    {
        var token = NewToken();
        token.Revoke(Now.AddMinutes(1));

        token.Revoke(Now.AddMinutes(5), Guid.NewGuid());

        Assert.Equal(Now.AddMinutes(1), token.RevokedAt);
        Assert.Null(token.ReplacedByTokenId);
    }

    [Fact]
    public void Issue_WithNonPositiveLifetime_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            RefreshToken.Issue(Guid.NewGuid(), "hash", Guid.NewGuid(), Now, TimeSpan.Zero));
    }
}
