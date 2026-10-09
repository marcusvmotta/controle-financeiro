using Microsoft.AspNetCore.Identity;

namespace FinanceControl.Infrastructure.Identity;

/// <summary>
/// Usuário do sistema. Herda de IdentityUser (e-mail, hash da senha, controle de bloqueio...)
/// e adiciona os campos do nosso domínio. Chave primária Guid (UUID v7, ordenável por tempo).
/// </summary>
public sealed class AppUser : IdentityUser<Guid>
{
    public AppUser()
    {
        Id = Guid.CreateVersion7();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public string Name { get; set; } = string.Empty;

    /// <summary>Usuário de demonstração (Módulo 6): não pode trocar senha nem ser excluído.</summary>
    public bool IsDemo { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
