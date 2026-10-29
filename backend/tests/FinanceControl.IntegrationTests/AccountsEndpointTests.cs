using System.Net;
using System.Net.Http.Json;
using FinanceControl.Application.Features.Accounts;
using FinanceControl.Domain.Enums;
using FinanceControl.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FinanceControl.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class AccountsEndpointTests(ApiFactory factory)
{
    private static AccountRequest NewAccount(string name = "Nubank", decimal initialBalance = 1500m) =>
        new(name, AccountType.Checking, initialBalance, "#8A05BE", "account_balance");

    private static async Task<AccountResponse> CreateAsync(HttpClient client, AccountRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/accounts", request, TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<AccountResponse>(TestJson.Options))!;
    }

    [Fact]
    public async Task Create_ValidAccount_Returns201WithLocationAndBalance()
    {
        var user = await TestUsers.CreateAsync(factory);

        var response = await user.Client.PostAsJsonAsync("/api/accounts", NewAccount(initialBalance: 1234.56m), TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<AccountResponse>(TestJson.Options);
        Assert.NotNull(account);
        Assert.Equal("Nubank", account.Name);
        Assert.Equal(AccountType.Checking, account.Type);
        Assert.Equal(1234.56m, account.InitialBalance);
        Assert.Equal(1234.56m, account.CurrentBalance); // sem transações ainda (M3)
        Assert.Equal($"/api/accounts/{account.Id}", response.Headers.Location?.AbsolutePath);
    }

    [Fact]
    public async Task Create_DuplicateNameIgnoringCaseAndSpaces_Returns409()
    {
        var user = await TestUsers.CreateAsync(factory);
        await CreateAsync(user.Client, NewAccount("Nubank"));

        var response = await user.Client.PostAsJsonAsync("/api/accounts", NewAccount("  NUBANK "), TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_SameNameAsAnotherUsersAccount_IsAllowed()
    {
        var alice = await TestUsers.CreateAsync(factory);
        var bob = await TestUsers.CreateAsync(factory);
        await CreateAsync(alice.Client, NewAccount("Carteira"));

        var response = await bob.Client.PostAsJsonAsync("/api/accounts", NewAccount("Carteira"), TestJson.Options);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_InvalidData_Returns400WithFieldErrors()
    {
        var user = await TestUsers.CreateAsync(factory);
        var invalid = new AccountRequest("", AccountType.Savings, 10.005m, "vermelho", "skull");

        var response = await user.Client.PostAsJsonAsync("/api/accounts", invalid, TestJson.Options);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("name", problem.Errors.Keys);
        Assert.Contains("initialBalance", problem.Errors.Keys);
        Assert.Contains("color", problem.Errors.Keys);
        Assert.Contains("icon", problem.Errors.Keys);
    }

    [Fact]
    public async Task Create_UnknownEnumValue_Returns400InPortugueseWithoutInternalNames()
    {
        var user = await TestUsers.CreateAsync(factory);
        var json = new StringContent(
            """{"name":"X","type":"Bitcoin","initialBalance":0,"color":"#000000","icon":"wallet"}""",
            System.Text.Encoding.UTF8, "application/json");

        var response = await user.Client.PostAsync("/api/accounts", json);
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("\"type\"", body);
        Assert.Contains("inválido", body);
        Assert.DoesNotContain("FinanceControl", body);
        Assert.DoesNotContain("$.", body);
    }

    [Fact]
    public async Task List_HidesArchivedByDefault_AndTotalIgnoresArchived()
    {
        var user = await TestUsers.CreateAsync(factory);
        await CreateAsync(user.Client, NewAccount("Corrente", 1000m));
        await CreateAsync(user.Client, NewAccount("Poupança", 500.50m));
        var old = await CreateAsync(user.Client, NewAccount("Conta antiga", 99m));
        await user.Client.PostAsync($"/api/accounts/{old.Id}/archive", null);

        var active = await user.Client.GetFromJsonAsync<AccountListResponse>("/api/accounts", TestJson.Options);
        var all = await user.Client.GetFromJsonAsync<AccountListResponse>("/api/accounts?includeArchived=true", TestJson.Options);

        Assert.Equal(2, active!.Items.Count);
        Assert.Equal(1500.50m, active.TotalBalance);
        Assert.Equal(3, all!.Items.Count);
        Assert.Equal(1500.50m, all.TotalBalance); // arquivada aparece, mas não soma
        Assert.True(all.Items.Single(a => a.Id == old.Id).IsArchived);
    }

    [Fact]
    public async Task Unarchive_BringsAccountBack()
    {
        var user = await TestUsers.CreateAsync(factory);
        var account = await CreateAsync(user.Client, NewAccount());
        await user.Client.PostAsync($"/api/accounts/{account.Id}/archive", null);

        var response = await user.Client.PostAsync($"/api/accounts/{account.Id}/unarchive", null);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var list = await user.Client.GetFromJsonAsync<AccountListResponse>("/api/accounts", TestJson.Options);
        Assert.Contains(list!.Items, a => a.Id == account.Id);
    }

    [Fact]
    public async Task Update_ChangesFields_AndAllowsChangingCaseOfOwnName()
    {
        var user = await TestUsers.CreateAsync(factory);
        var account = await CreateAsync(user.Client, NewAccount("nubank"));

        var response = await user.Client.PutAsJsonAsync($"/api/accounts/{account.Id}",
            new AccountRequest("Nubank", AccountType.CreditCard, -320.10m, "#000000", "credit_card"), TestJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<AccountResponse>(TestJson.Options);
        Assert.Equal("Nubank", updated!.Name);
        Assert.Equal(AccountType.CreditCard, updated.Type);
        Assert.Equal(-320.10m, updated.CurrentBalance);
    }

    [Fact]
    public async Task Update_ToNameOfAnotherOwnAccount_Returns409()
    {
        var user = await TestUsers.CreateAsync(factory);
        await CreateAsync(user.Client, NewAccount("Corrente"));
        var savings = await CreateAsync(user.Client, NewAccount("Poupança"));

        var response = await user.Client.PutAsJsonAsync($"/api/accounts/{savings.Id}", NewAccount("corrente"), TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204_ThenGetReturns404()
    {
        var user = await TestUsers.CreateAsync(factory);
        var account = await CreateAsync(user.Client, NewAccount());

        var delete = await user.Client.DeleteAsync($"/api/accounts/{account.Id}");
        var get = await user.Client.GetAsync($"/api/accounts/{account.Id}");

        Assert.Equal(HttpStatusCode.NoContent, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
    }

    [Fact]
    public async Task WithoutToken_Returns401()
    {
        var anonymous = factory.CreateApiClient();

        var response = await anonymous.GetAsync("/api/accounts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
