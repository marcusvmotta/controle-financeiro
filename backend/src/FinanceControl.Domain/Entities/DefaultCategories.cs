using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Categorias que todo usuário novo recebe (PRD RF-03). São COPIADAS para cada usuário,
/// para que ele possa renomear, arquivar ou excluir livremente (TRD 02-modelo-de-dados, "Seeds").
/// Ícones: nomes do Material Symbols.
/// </summary>
public static class DefaultCategories
{
    public sealed record Template(string Name, CategoryType Type, string Color, string Icon);

    public static IReadOnlyList<Template> All { get; } =
    [
        // Despesas
        new("Alimentação", CategoryType.Expense, "#4CAF50", "restaurant"),
        new("Moradia", CategoryType.Expense, "#3F51B5", "home"),
        new("Transporte", CategoryType.Expense, "#FF9800", "directions_car"),
        new("Saúde", CategoryType.Expense, "#E91E63", "medical_services"),
        new("Lazer", CategoryType.Expense, "#9C27B0", "sports_esports"),
        new("Educação", CategoryType.Expense, "#00BCD4", "school"),
        new("Compras", CategoryType.Expense, "#795548", "shopping_bag"),
        new("Assinaturas", CategoryType.Expense, "#607D8B", "subscriptions"),
        new("Outros", CategoryType.Expense, "#9E9E9E", "more_horiz"),

        // Receitas
        new("Salário", CategoryType.Income, "#2E7D32", "payments"),
        new("Freelance", CategoryType.Income, "#0288D1", "work"),
        new("Rendimentos", CategoryType.Income, "#F9A825", "savings"),
        new("Outros", CategoryType.Income, "#9E9E9E", "more_horiz"),
    ];

    public static IEnumerable<Category> CreateFor(Guid userId) =>
        All.Select(t => Category.Create(userId, t.Name, t.Type, t.Color, t.Icon));
}
