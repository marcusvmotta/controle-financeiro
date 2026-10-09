using FinanceControl.Application.Abstractions;
using FinanceControl.Application.Common.Exceptions;
using FinanceControl.Application.Features.Auth;
using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinanceControl.Infrastructure.Identity;

/// <summary>
/// Implementação dos casos de uso de autenticação (TRD modulos/01-autenticacao.md e 04-seguranca.md).
/// </summary>
public sealed class AuthService(
    UserManager<AppUser> userManager,
    AppDbContext db,
    JwtTokenGenerator tokenGenerator,
    ICurrentUser currentUser,
    TimeProvider timeProvider,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    // Mesma mensagem para "e-mail não existe", "senha errada" e "conta bloqueada":
    // assim um atacante não descobre quais e-mails estão cadastrados.
    private const string InvalidCredentialsMessage = "E-mail ou senha inválidos.";
    private const string InvalidSessionMessage = "Sessão inválida ou expirada. Faça login novamente.";

    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = NormalizeEmail(request.Email);

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new ConflictException("Este e-mail já está cadastrado.");
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            Name = request.Name.Trim(),
            CreatedAt = timeProvider.GetUtcNow(),
        };

        // CreateAsync valida a senha com as regras do Identity e grava só o hash (PBKDF2), nunca a senha.
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            // Duas requisições simultâneas com o mesmo e-mail: a segunda cai aqui.
            if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
            {
                throw new ConflictException("Este e-mail já está cadastrado.");
            }

            throw new BusinessRuleException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        logger.LogInformation("Usuário {UserId} cadastrado", user.Id);
        return await IssueTokensAsync(user, familyId: Guid.CreateVersion7(), cancellationToken);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(NormalizeEmail(request.Email))
            ?? throw new UnauthorizedException(InvalidCredentialsMessage);

        if (await userManager.IsLockedOutAsync(user))
        {
            logger.LogWarning("Login bloqueado (lockout) para o usuário {UserId}", user.Id);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            // Conta a falha; na 5ª seguida, o Identity bloqueia a conta por 15 minutos.
            await userManager.AccessFailedAsync(user);
            throw new UnauthorizedException(InvalidCredentialsMessage);
        }

        await userManager.ResetAccessFailedCountAsync(user);
        return await IssueTokensAsync(user, familyId: Guid.CreateVersion7(), cancellationToken);
    }

    public async Task<AuthResult> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var hash = JwtTokenGenerator.HashRefreshToken(refreshToken);

        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken)
            ?? throw new UnauthorizedException(InvalidSessionMessage);

        if (stored.IsRevoked)
        {
            // Um token já trocado está sendo usado de novo: provavelmente foi roubado.
            // Revogamos a família inteira, derrubando o atacante E o usuário legítimo (que fará login de novo).
            logger.LogWarning(
                "Reuso de refresh token detectado para o usuário {UserId}; revogando a família {FamilyId}",
                stored.UserId, stored.FamilyId);

            await db.RefreshTokens
                .Where(t => t.FamilyId == stored.FamilyId && t.RevokedAt == null)
                .ExecuteUpdateAsync(setters => setters.SetProperty(t => t.RevokedAt, now), cancellationToken);

            throw new UnauthorizedException(InvalidSessionMessage);
        }

        if (stored.IsExpired(now))
        {
            throw new UnauthorizedException(InvalidSessionMessage);
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString())
            ?? throw new UnauthorizedException(InvalidSessionMessage);

        // Rotação: o token usado é revogado e substituído por um novo, na mesma família.
        return await IssueTokensAsync(user, stored.FamilyId, cancellationToken, replacing: stored);
    }

    public async Task LogoutAsync(string? refreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var hash = JwtTokenGenerator.HashRefreshToken(refreshToken);
        var stored = await db.RefreshTokens.SingleOrDefaultAsync(t => t.TokenHash == hash, cancellationToken);
        if (stored is null)
        {
            return;
        }

        stored.Revoke(timeProvider.GetUtcNow());
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(currentUser.UserId.ToString())
            ?? throw new UnauthorizedException(InvalidSessionMessage);

        return ToResponse(user);
    }

    private async Task<AuthResult> IssueTokensAsync(
        AppUser user, Guid familyId, CancellationToken cancellationToken, RefreshToken? replacing = null)
    {
        var now = timeProvider.GetUtcNow();
        var (accessToken, accessExpiresAt) = tokenGenerator.CreateAccessToken(user);

        var rawRefreshToken = JwtTokenGenerator.CreateRefreshToken();
        var refreshEntity = RefreshToken.Issue(
            user.Id,
            JwtTokenGenerator.HashRefreshToken(rawRefreshToken),
            familyId,
            now,
            TimeSpan.FromDays(jwtOptions.Value.RefreshTokenDays));

        replacing?.Revoke(now, refreshEntity.Id);
        db.RefreshTokens.Add(refreshEntity);
        await db.SaveChangesAsync(cancellationToken);

        return new AuthResult(
            new AuthResponse(accessToken, accessExpiresAt, ToResponse(user)),
            rawRefreshToken,
            refreshEntity.ExpiresAt);
    }

    private static UserResponse ToResponse(AppUser user) =>
        new(user.Id, user.Name, user.Email ?? string.Empty, user.IsDemo);

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
