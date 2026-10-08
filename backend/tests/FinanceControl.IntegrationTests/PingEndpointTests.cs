using System.Net;
using System.Net.Http.Json;
using FinanceControl.Api.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FinanceControl.IntegrationTests;

/// <summary>
/// WebApplicationFactory sobe a API inteira em memória, sem abrir porta de rede.
/// IClassFixture faz a mesma instância ser reaproveitada por todos os testes desta classe.
/// </summary>
public sealed class PingEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetPing_ReturnsPong()
    {
        // Act
        var response = await _client.GetAsync("/api/ping");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<PingResponse>();
        Assert.NotNull(body);
        Assert.Equal("pong", body.Message);
    }

    [Fact]
    public async Task GetHealthLive_ReturnsHealthy_WithoutTouchingTheDatabase()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", await response.Content.ReadAsStringAsync());
    }
}
