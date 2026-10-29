using FinanceControl.Application.Abstractions;
using FinanceControl.Application.Common.Exceptions;
using FinanceControl.Domain.Common;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Application.Features.Categories;

/// <summary>Casos de uso de categorias (TRD modulos/03-categorias.md).</summary>
public sealed class CategoryService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<IReadOnlyList<CategoryResponse>> ListAsync(
        CategoryType? type, bool includeArchived, CancellationToken cancellationToken) =>
        await db.Categories
            .AsNoTracking()
            .Where(c => type == null || c.Type == type)
            .Where(c => includeArchived || !c.IsArchived)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.IsArchived)
            .ThenBy(c => c.Name)
            .Select(c => ToResponse(c))
            .ToListAsync(cancellationToken);

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsAvailableAsync(request.Name, request.Type, exceptId: null, cancellationToken);

        var category = Category.Create(currentUser.UserId, request.Name, request.Type, request.Color, request.Icon);
        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await FindAsync(id, cancellationToken);
        await EnsureNameIsAvailableAsync(request.Name, category.Type, exceptId: id, cancellationToken);

        category.Update(request.Name, request.Color, request.Icon);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var category = await FindAsync(id, cancellationToken);
        // M3: categorias com transações não poderão ser excluídas (409, "arquive em vez disso").
        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetArchivedAsync(Guid id, bool archived, CancellationToken cancellationToken)
    {
        var category = await FindAsync(id, cancellationToken);
        if (archived)
        {
            category.Archive();
        }
        else
        {
            category.Unarchive();
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<Category> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Categories.FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
        ?? throw new NotFoundException("Categoria não encontrada.");

    /// <summary>O nome é único por tipo: pode existir "Outros" em despesas E em receitas.</summary>
    private async Task EnsureNameIsAvailableAsync(
        string name, CategoryType type, Guid? exceptId, CancellationToken cancellationToken)
    {
        var normalized = NameNormalizer.Normalize(name);
        var taken = await db.Categories.AnyAsync(
            c => c.NormalizedName == normalized && c.Type == type && c.Id != exceptId, cancellationToken);

        if (taken)
        {
            var kind = type == CategoryType.Expense ? "despesa" : "receita";
            throw new ConflictException($"Você já tem uma categoria de {kind} chamada \"{name.Trim()}\".");
        }
    }

    // TransactionCount chega com as transações no M3.
    private static CategoryResponse ToResponse(Category c) =>
        new(c.Id, c.Name, c.Type, c.Color, c.Icon, c.IsArchived, TransactionCount: 0);
}
