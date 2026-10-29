using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

/// <summary>Tabela accounts (TRD 02-modelo-de-dados).</summary>
internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(60).IsRequired();
        builder.Property(a => a.NormalizedName).HasMaxLength(60).IsRequired();
        // Enum gravado como texto ("Checking"): legível no banco e seguro se o enum for reordenado.
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
        // numeric(18,2): dinheiro com 2 casas decimais exatas (ADR-012).
        builder.Property(a => a.InitialBalance).HasPrecision(18, 2);
        builder.Property(a => a.Color).HasMaxLength(7).IsRequired();
        builder.Property(a => a.Icon).HasMaxLength(40).IsRequired();

        // Nome único por usuário, sem diferenciar maiúsculas (a garantia final é do banco).
        builder.HasIndex(a => new { a.UserId, a.NormalizedName }).IsUnique();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

/// <summary>Tabela categories.</summary>
internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).HasMaxLength(40).IsRequired();
        builder.Property(c => c.NormalizedName).HasMaxLength(40).IsRequired();
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(10);
        builder.Property(c => c.Color).HasMaxLength(7).IsRequired();
        builder.Property(c => c.Icon).HasMaxLength(40).IsRequired();

        // Único por usuário E tipo: "Outros" pode existir em despesas e em receitas.
        builder.HasIndex(c => new { c.UserId, c.Type, c.NormalizedName }).IsUnique();

        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
