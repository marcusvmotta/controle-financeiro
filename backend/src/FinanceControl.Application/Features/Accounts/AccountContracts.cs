using FinanceControl.Domain.Enums;

namespace FinanceControl.Application.Features.Accounts;

/// <summary>Usado tanto para criar quanto para editar (os campos são os mesmos).</summary>
public sealed record AccountRequest(string Name, AccountType Type, decimal InitialBalance, string Color, string Icon);

public sealed record AccountResponse(
    Guid Id,
    string Name,
    AccountType Type,
    decimal InitialBalance,
    decimal CurrentBalance,
    string Color,
    string Icon,
    bool IsArchived);

/// <summary>Lista de contas + saldo total das contas NÃO arquivadas. Sem paginação: uma pessoa tem poucas contas.</summary>
public sealed record AccountListResponse(IReadOnlyList<AccountResponse> Items, decimal TotalBalance);
