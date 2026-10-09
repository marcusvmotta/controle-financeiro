using System.Security.Claims;
using FinanceControl.Application.Abstractions;
using FinanceControl.Application.Common.Exceptions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace FinanceControl.Api.Infrastructure;

/// <summary>Lê o usuário autenticado a partir do JWT da requisição atual (claim "sub").</summary>
public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true;

    public Guid UserId =>
        Guid.TryParse(Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
            ? id
            : throw new UnauthorizedException("Usuário não autenticado.");
}
