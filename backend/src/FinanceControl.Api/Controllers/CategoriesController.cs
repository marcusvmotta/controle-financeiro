using FinanceControl.Application.Features.Categories;
using FinanceControl.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.Api.Controllers;

/// <summary>Categorias de receita e despesa do usuário logado (TRD modulos/03-categorias.md).</summary>
[ApiController]
[Route("api/categories")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public sealed class CategoriesController(CategoryService categories) : ControllerBase
{
    /// <summary>Lista as categorias. Filtro opcional por tipo: ?type=Expense ou ?type=Income.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> List(
        [FromQuery] CategoryType? type, [FromQuery] bool includeArchived, CancellationToken cancellationToken) =>
        Ok(await categories.ListAsync(type, includeArchived, cancellationToken));

    [HttpPost]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> Create(
        CreateCategoryRequest request, IValidator<CreateCategoryRequest> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var category = await categories.CreateAsync(request, cancellationToken);
        return Created($"/api/categories/{category.Id}", category);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CategoryResponse>> Update(
        Guid id, UpdateCategoryRequest request, IValidator<UpdateCategoryRequest> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        return Ok(await categories.UpdateAsync(id, request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await categories.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        await categories.SetArchivedAsync(id, archived: true, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/unarchive")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Unarchive(Guid id, CancellationToken cancellationToken)
    {
        await categories.SetArchivedAsync(id, archived: false, cancellationToken);
        return NoContent();
    }
}
