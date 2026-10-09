using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence;

/// <summary>
/// Ponto de acesso ao banco via EF Core. Cada DbSet vira uma tabela.
/// Herda de IdentityUserContext: traz as tabelas de usuário do Identity, sem as de "roles" (não usamos papéis).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityUserContext<AppUser, Guid>(options)
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Primeiro o Identity configura as tabelas dele; depois aplicamos as nossas configurações por cima.
        base.OnModelCreating(modelBuilder);

        // Aplica todas as classes IEntityTypeConfiguration<T> deste projeto (pasta Configurations).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
