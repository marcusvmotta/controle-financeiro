using FinanceControl.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Application.Abstractions;

/// <summary>
/// O que os services da Application enxergam do banco (ADR-001: sem repositórios por cima do EF Core).
/// A implementação é o AppDbContext, na Infrastructure.
/// Todo DbSet de entidade IUserOwned já vem filtrado pelo usuário logado (query filter global).
/// </summary>
public interface IAppDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Category> Categories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
