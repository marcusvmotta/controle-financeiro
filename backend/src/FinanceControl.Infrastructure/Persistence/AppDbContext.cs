using Microsoft.EntityFrameworkCore;

namespace FinanceControl.Infrastructure.Persistence;

/// <summary>
/// Ponto de acesso ao banco via EF Core. Cada DbSet (adicionados a partir do M1) vira uma tabela.
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as classes IEntityTypeConfiguration<T> deste projeto (mapeamento das tabelas).
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
