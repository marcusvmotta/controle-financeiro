namespace FinanceControl.Domain.Enums;

/// <summary>Tipo de conta (PRD RF-02). Gravado como texto no banco.</summary>
public enum AccountType
{
    Checking,
    Savings,
    Cash,
    CreditCard,
    Other,
}

/// <summary>Uma categoria classifica receitas OU despesas, nunca as duas (PRD RF-03).</summary>
public enum CategoryType
{
    Income,
    Expense,
}
