using FinanceControl.Application.Features.Auth;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.Api.Controllers;

/// <summary>
/// Dados do usuário logado. Sem [AllowAnonymous]: a política padrão exige autenticação.
/// </summary>
[ApiController]
[Route("api/me")]
public sealed class MeController(IAuthService authService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserResponse>> Get(CancellationToken cancellationToken) =>
        Ok(await authService.GetCurrentUserAsync(cancellationToken));
}
