using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinanceControl.Infrastructure.Persistence;

/// <summary>
/// Usada só pelas ferramentas do EF ("dotnet ef migrations add"), em tempo de desenvolvimento.
/// Assim as ferramentas não precisam subir a API inteira (nem ter Jwt:Key configurada) para gerar migrations.
/// Gerar uma migration não conecta no banco; a connection string aqui é só um formato válido.
/// </summary>
internal sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=finance;Username=finance;Password=finance_dev")
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }
}
