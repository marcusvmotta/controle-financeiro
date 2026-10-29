using FinanceControl.Application.Features.Accounts;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.Api.Controllers;

/// <summary>Contas e carteiras do usuário logado (TRD modulos/02-contas.md).</summary>
[ApiController]
[Route("api/accounts")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class AccountsController(AccountService accounts, IValidator<AccountRequest> validator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AccountListResponse>> List(
        [FromQuery] bool includeArchived, CancellationToken cancellationToken) =>
        Ok(await accounts.ListAsync(includeArchived, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Get(Guid id, CancellationToken cancellationToken) =>
        Ok(await accounts.GetAsync(id, cancellationToken));

    [HttpPost]
    [ProducesResponseType<AccountResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountResponse>> Create(AccountRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var account = await accounts.CreateAsync(request, cancellationToken);
        // 201 + header Location apontando para o GET da conta criada (TRD 03-api).
        return CreatedAtAction(nameof(Get), new { id = account.Id }, account);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AccountResponse>> Update(
        Guid id, AccountRequest request, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await accounts.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await accounts.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await accounts.SetArchivedAsync(id, archived: true, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unarchive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(Guid id, CancellationToken cancellationToken)
    {
        await accounts.SetArchivedAsync(id, archived: false, cancellationToken);
        return NoContent();
    }
}
