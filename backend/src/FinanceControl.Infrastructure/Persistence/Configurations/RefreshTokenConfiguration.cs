using FinanceControl.Domain.Entities;
using FinanceControl.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceControl.Infrastructure.Persistence.Configurations;

/// <summary>
/// Mapeamento da entidade RefreshToken para a tabela refresh_tokens.
/// Fica aqui (e não no Domain) para o Domain continuar sem depender do EF Core.
/// </summary>
internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");
        builder.HasKey(t => t.Id);

        // SHA-256 em hexadecimal = 64 caracteres. Índice único: a busca no refresh é por aqui.
        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.HasIndex(t => t.TokenHash).IsUnique();

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => t.FamilyId);

        // Se o usuário for excluído, os tokens dele vão junto.
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Propriedade calculada (não é coluna).
        builder.Ignore(t => t.IsRevoked);
    }
}
