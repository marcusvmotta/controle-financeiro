using System.Net;
using System.Net.Http.Json;
using FinanceControl.Api.Controllers;
using FinanceControl.IntegrationTests.Infrastructure;

namespace FinanceControl.IntegrationTests;

/// <summary>
/// ApiFactory sobe a API inteira em memória (sem abrir porta de rede), com um PostgreSQL real em container.
/// [Collection] faz a mesma instância ser reaproveitada por todas as classes de teste da coleção.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class PingEndpointTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateApiClient();

    [Fact]
    public async Task GetPing_ReturnsPong_WithoutAuthentication()
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
    public async Task GetHealthLive_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetHealthReady_WithDatabaseUp_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
