using System.Linq.Expressions;
using FinanceControl.Application.Abstractions;
using FinanceControl.Domain.Common;
using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence;

/// <summary>
/// Ponto de acesso ao banco via EF Core. Cada DbSet vira uma tabela.
/// Herda de IdentityUserContext: traz as tabelas de usuário do Identity, sem as de "roles" (não usamos papéis).
/// </summary>
public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IdentityUserContext<AppUser, Guid>(options), IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Usuário usado pelos query filters. O EF Core lê esta propriedade A CADA consulta
    /// (ela vira um parâmetro do SQL), então cada requisição filtra pelo seu próprio usuário.
    /// Sem usuário logado, Guid.Empty: nenhuma linha bate, e nada vaza.
    /// </summary>
    private Guid CurrentUserId => currentUser.IsAuthenticated ? currentUser.UserId : Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primeiro o Identity configura as tabelas dele; depois aplicamos as nossas configurações por cima.
        base.OnModelCreating(modelBuilder);

        // Aplica todas as classes IEntityTypeConfiguration<T> deste projeto (pasta Configurations).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        ApplyUserOwnershipFilters(modelBuilder);
    }

    /// <summary>
    /// ISOLAMENTO ENTRE USUÁRIOS (TRD 04-seguranca): toda entidade IUserOwned recebe o filtro
    /// "UserId == usuário logado". Nenhum service precisa (nem pode esquecer de) filtrar à mão.
    /// </summary>
    private void ApplyUserOwnershipFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                     .Where(t => typeof(IUserOwned).IsAssignableFrom(t.ClrType)))
        {
            // Monta, para cada tipo T, a expressão: (T e) => e.UserId == this.CurrentUserId
            var entity = Expression.Parameter(entityType.ClrType, "e");
            var filter = Expression.Lambda(
                Expression.Equal(
                    Expression.Property(entity, nameof(IUserOwned.UserId)),
                    Expression.Property(Expression.Constant(this), nameof(CurrentUserId))),
                entity);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    /// <summary>Preenche CreatedAt/UpdatedAt automaticamente, para nenhum service precisar lembrar.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = timeProvider.GetUtcNow();

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(IAuditable.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditable.UpdatedAt)).CurrentValue = now;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
