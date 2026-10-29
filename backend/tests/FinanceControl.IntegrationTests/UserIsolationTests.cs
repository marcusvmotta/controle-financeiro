using System.Net;
using System.Net.Http.Json;
using FinanceControl.Application.Features.Accounts;
using FinanceControl.Application.Features.Categories;
using FinanceControl.Domain.Enums;
using FinanceControl.IntegrationTests.Infrastructure;

namespace FinanceControl.IntegrationTests;

/// <summary>
/// TESTE OBRIGATÓRIO Nº 1 (TRD 08-testes, RNF-02): um usuário nunca acessa dados de outro.
/// Bob tenta ler, alterar, arquivar e excluir os dados de Alice: tudo deve dar 404
/// (nunca 403, para não revelar que o recurso existe), e os dados de Alice ficam intactos.
/// Ao criar novos módulos (transações, orçamentos...), adicione os endpoints deles aqui.
/// </summary>
[Collection(ApiCollection.Name)]
public sealed class UserIsolationTests(ApiFactory factory)
{
    [Fact]
    public async Task OtherUser_CannotSeeOrTouchAccounts()
    {
        var alice = await TestUsers.CreateAsync(factory);
        var bob = await TestUsers.CreateAsync(factory);
        var created = await alice.Client.PostAsJsonAsync("/api/accounts",
            new AccountRequest("Conta da Alice", AccountType.Checking, 1000m, "#000000", "account_balance"), TestJson.Options);
        var aliceAccount = (await created.Content.ReadFromJsonAsync<AccountResponse>(TestJson.Options))!;
        var url = $"/api/accounts/{aliceAccount.Id}";

        // Bob tenta de tudo.
        var attempts = new[]
        {
            await bob.Client.GetAsync(url),
            await bob.Client.PutAsJsonAsync(url,
                new AccountRequest("Hackeada", AccountType.Cash, 0m, "#FFFFFF", "wallet"), TestJson.Options),
            await bob.Client.PostAsync($"{url}/archive", null),
            await bob.Client.PostAsync($"{url}/unarchive", null),
            await bob.Client.DeleteAsync(url),
        };

        Assert.All(attempts, response => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode));

        // A lista de Bob não mostra a conta de Alice...
        var bobList = await bob.Client.GetFromJsonAsync<AccountListResponse>("/api/accounts?includeArchived=true", TestJson.Options);
        Assert.DoesNotContain(bobList!.Items, a => a.Id == aliceAccount.Id);

        // ...e a conta de Alice continua exatamente como era.
        var stillThere = await alice.Client.GetFromJsonAsync<AccountResponse>(url, TestJson.Options);
        Assert.Equal(aliceAccount, stillThere);
    }

    [Fact]
    public async Task OtherUser_CannotSeeOrTouchCategories()
    {
        var alice = await TestUsers.CreateAsync(factory);
        var bob = await TestUsers.CreateAsync(factory);
        var created = await alice.Client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("Só da Alice", CategoryType.Expense, "#000000", "pets"), TestJson.Options);
        var aliceCategory = (await created.Content.ReadFromJsonAsync<CategoryResponse>(TestJson.Options))!;
        var url = $"/api/categories/{aliceCategory.Id}";

        var attempts = new[]
        {
            await bob.Client.PutAsJsonAsync(url, new UpdateCategoryRequest("Hackeada", "#FFFFFF", "pets"), TestJson.Options),
            await bob.Client.PostAsync($"{url}/archive", null),
            await bob.Client.PostAsync($"{url}/unarchive", null),
            await bob.Client.DeleteAsync(url),
        };

        Assert.All(attempts, response => Assert.Equal(HttpStatusCode.NotFound, response.StatusCode));

        var bobList = await bob.Client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories?includeArchived=true", TestJson.Options);
        Assert.DoesNotContain(bobList!, c => c.Id == aliceCategory.Id);

        var aliceList = await alice.Client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories", TestJson.Options);
        Assert.Contains(aliceList!, c => c == aliceCategory);
    }

    [Fact]
    public async Task EachUser_GetsTheirOwnCopyOfDefaultCategories()
    {
        var alice = await TestUsers.CreateAsync(factory);
        var bob = await TestUsers.CreateAsync(factory);

        var aliceIds = (await alice.Client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories", TestJson.Options))!
            .Select(c => c.Id);
        var bobIds = (await bob.Client.GetFromJsonAsync<List<CategoryResponse>>("/api/categories", TestJson.Options))!
            .Select(c => c.Id);

        Assert.Empty(aliceIds.Intersect(bobIds));
    }
}
