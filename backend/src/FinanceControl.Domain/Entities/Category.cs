using FinanceControl.Domain.Common;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Classifica receitas ou despesas (PRD RF-03, TRD modulos/03-categorias.md).
/// O tipo é definido na criação e nunca muda: mudar invalidaria as transações já lançadas.
/// </summary>
public sealed class Category : IUserOwned, IAuditable
{
    private Category()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
        Color = string.Empty;
        Icon = string.Empty;
    }

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public string NormalizedName { get; private set; }
    public CategoryType Type { get; private set; }
    public string Color { get; private set; }
    public string Icon { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Category Create(Guid userId, string name, CategoryType type, string color, string icon)
    {
        var category = new Category { Id = Guid.CreateVersion7(), UserId = userId, Type = type };
        category.Update(name, color, icon);
        return category;
    }

    /// <summary>Atualiza o que pode mudar. Repare: não existe parâmetro para o tipo.</summary>
    public void Update(string name, string color, string icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        NormalizedName = NameNormalizer.Normalize(name);
        Color = color;
        Icon = icon;
    }

    public void Archive() => IsArchived = true;

    public void Unarchive() => IsArchived = false;
}
