using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace FinanceControl.IntegrationTests.Infrastructure;

/// <summary>
/// Sobe a API em memória apontando para um PostgreSQL real, num container Docker descartável (Testcontainers).
/// Uma instância é compartilhada por todos os testes da coleção "Api" (ver ApiCollection).
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string JwtKey = "integration-tests-signing-key-0123456789abcdef";
    public const string JwtIssuer = "finance-control";

    private readonly PostgreSqlContainer _database = new PostgreSqlBuilder("postgres:18-alpine").Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ambiente "Testing": não carrega o appsettings.Development.json.
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Default", _database.GetConnectionString());
        builder.UseSetting("Jwt:Key", JwtKey);
        builder.UseSetting("Database:MigrateOnStartup", "true");
        // Os testes fazem muitos logins seguidos do "mesmo IP"; o rate limit tem teste próprio.
        builder.UseSetting("RateLimiting:AuthPermitLimit", "100000");
    }

    public Task InitializeAsync() => _database.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _database.DisposeAsync();
    }

    /// <summary>Cliente HTTP sem gerenciamento automático de cookies: os testes controlam os cookies explicitamente.</summary>
    public HttpClient CreateApiClient() =>
        CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });
}

[CollectionDefinition(Name)]
public sealed class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Name = "Api";
}
