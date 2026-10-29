using FinanceControl.Application.Abstractions;
using FinanceControl.Application.Common.Exceptions;
using FinanceControl.Domain.Common;
using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Application.Features.Accounts;

/// <summary>
/// Casos de uso de contas (TRD modulos/02-contas.md).
/// Repare que nenhuma consulta filtra por usuário "na mão": o query filter global do AppDbContext
/// já garante que db.Accounts só contém as contas do usuário logado.
/// </summary>
public sealed class AccountService(IAppDbContext db, ICurrentUser currentUser)
{
    public async Task<AccountListResponse> ListAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var accounts = await db.Accounts
            .AsNoTracking() // só leitura: o EF não precisa acompanhar mudanças, fica mais rápido
            .Where(a => includeArchived || !a.IsArchived)
            .OrderBy(a => a.IsArchived)
            .ThenBy(a => a.Name)
            .Select(a => ToResponse(a))
            .ToListAsync(cancellationToken);

        var totalBalance = accounts.Where(a => !a.IsArchived).Sum(a => a.CurrentBalance);
        return new AccountListResponse(accounts, totalBalance);
    }

    public async Task<AccountResponse> GetAsync(Guid id, CancellationToken cancellationToken) =>
        ToResponse(await FindAsync(id, cancellationToken));

    public async Task<AccountResponse> CreateAsync(AccountRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsAvailableAsync(request.Name, exceptId: null, cancellationToken);

        var account = Account.Create(
            currentUser.UserId, // o dono vem do token, NUNCA do corpo da requisição
            request.Name, request.Type, request.InitialBalance, request.Color, request.Icon);

        db.Accounts.Add(account);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(account);
    }

    public async Task<AccountResponse> UpdateAsync(Guid id, AccountRequest request, CancellationToken cancellationToken)
    {
        var account = await FindAsync(id, cancellationToken);
        await EnsureNameIsAvailableAsync(request.Name, exceptId: id, cancellationToken);

        account.Update(request.Name, request.Type, request.InitialBalance, request.Color, request.Icon);
        await db.SaveChangesAsync(cancellationToken);
        return ToResponse(account);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await FindAsync(id, cancellationToken);
        // M3: contas com transações não poderão ser excluídas (409, "arquive em vez disso").
        db.Accounts.Remove(account);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SetArchivedAsync(Guid id, bool archived, CancellationToken cancellationToken)
    {
        var account = await FindAsync(id, cancellationToken);
        if (archived)
        {
            account.Archive();
        }
        else
        {
            account.Unarchive();
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Busca pelo id. Se a conta não existe OU é de outro usuário, a resposta é a mesma: 404.
    /// Assim ninguém consegue descobrir se um id existe "testando" ids de outras pessoas.
    /// </summary>
    private async Task<Account> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
        ?? throw new NotFoundException("Conta não encontrada.");

    private async Task EnsureNameIsAvailableAsync(string name, Guid? exceptId, CancellationToken cancellationToken)
    {
        var normalized = NameNormalizer.Normalize(name);
        var taken = await db.Accounts.AnyAsync(
            a => a.NormalizedName == normalized && a.Id != exceptId, cancellationToken);

        if (taken)
        {
            throw new ConflictException($"Você já tem uma conta chamada \"{name.Trim()}\".");
        }
    }

    // Saldo atual = saldo inicial + entradas - saídas (ADR-006).
    // As transações chegam no M3; até lá, o saldo atual é o próprio saldo inicial.
    private static AccountResponse ToResponse(Account a) =>
        new(a.Id, a.Name, a.Type, a.InitialBalance, a.InitialBalance, a.Color, a.Icon, a.IsArchived);
}
