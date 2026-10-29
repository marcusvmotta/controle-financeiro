using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.Features.Categories;

public sealed record CreateCategoryRequest(string Name, CategoryType Type, string Color, string Icon);

/// <summary>Sem o campo "type": o tipo de uma categoria não muda depois de criada.</summary>
public sealed record UpdateCategoryRequest(string Name, string Color, string Icon);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    CategoryType Type,
    string Color,
    string Icon,
    bool IsArchived,
    int TransactionCount);
