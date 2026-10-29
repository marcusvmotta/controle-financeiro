using FinanceControl.Domain.Common;
using FinanceControl.Domain.Enums;

namespace FinanceControl.Domain.Entities;

/// <summary>
/// Onde o dinheiro está: conta corrente, poupança, carteira... (PRD RF-02, TRD modulos/02-contas.md).
/// O saldo atual NÃO é guardado aqui: é calculado a partir das transações (ADR-006).
/// </summary>
public sealed class Account : IUserOwned, IAuditable
{
    private Account()
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
    public AccountType Type { get; private set; }

    /// <summary>Pode ser negativo (ex.: cartão de crédito com fatura em aberto).</summary>
    public decimal InitialBalance { get; private set; }

    public string Color { get; private set; }
    public string Icon { get; private set; }
    public bool IsArchived { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public static Account Create(Guid userId, string name, AccountType type, decimal initialBalance, string color, string icon)
    {
        var account = new Account { Id = Guid.CreateVersion7(), UserId = userId };
        account.Update(name, type, initialBalance, color, icon);
        return account;
    }

    public void Update(string name, AccountType type, decimal initialBalance, string color, string icon)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        NormalizedName = NameNormalizer.Normalize(name);
        Type = type;
        InitialBalance = initialBalance;
        Color = color;
        Icon = icon;
    }

    /// <summary>Arquivar esconde a conta das listas e seletores, mas mantém o histórico.</summary>
    public void Archive() => IsArchived = true;

    public void Unarchive() => IsArchived = false;
}
