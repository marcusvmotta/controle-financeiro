using System.Net;
using System.Net.Http.Json;
using FinanceControl.Application.Features.Categories;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;
using FinanceControl.IntegrationTests.Infrastructure;

namespace FinanceControl.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class CategoriesEndpointTests(ApiFactory factory)
{
    private static async Task<List<CategoryResponse>> ListAsync(HttpClient client, string query = "") =>
        (await client.GetFromJsonAsync<List<CategoryResponse>>($"/api/categories{query}", TestJson.Options))!;

    private static async Task<CategoryResponse> CreateAsync(HttpClient client, CreateCategoryRequest request)
    {
        var response = await client.PostAsJsonAsync("/api/categories", request, TestJson.Options);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<CategoryResponse>(TestJson.Options))!;
    }

    [Fact]
    public async Task NewUser_StartsWithDefaultCategories()
    {
        var user = await TestUsers.CreateAsync(factory);

        var categories = await ListAsync(user.Client);

        Assert.Equal(DefaultCategories.All.Count, categories.Count);
        Assert.Contains(categories, c => c is { Name: "Alimentação", Type: CategoryType.Expense });
        Assert.Contains(categories, c => c is { Name: "Salário", Type: CategoryType.Income });
    }

    [Fact]
    public async Task List_FiltersByType()
    {
        var user = await TestUsers.CreateAsync(factory);

        var income = await ListAsync(user.Client, "?type=Income");

        Assert.NotEmpty(income);
        Assert.All(income, c => Assert.Equal(CategoryType.Income, c.Type));
    }

    [Fact]
    public async Task Create_SameNameInSameType_Returns409_ButOtherTypeIsAllowed()
    {
        var user = await TestUsers.CreateAsync(factory);

        // "Alimentação" já existe como despesa (categoria padrão).
        var sameType = await user.Client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("ALIMENTAÇÃO", CategoryType.Expense, "#FF0000", "restaurant"), TestJson.Options);
        var otherType = await user.Client.PostAsJsonAsync("/api/categories",
            new CreateCategoryRequest("Alimentação", CategoryType.Income, "#FF0000", "restaurant"), TestJson.Options);

        Assert.Equal(HttpStatusCode.Conflict, sameType.StatusCode);
        Assert.Equal(HttpStatusCode.Created, otherType.StatusCode);
    }

    [Fact]
    public async Task Update_ChangesNameColorIcon_ButNeverTheType()
    {
        var user = await TestUsers.CreateAsync(factory);
        var pets = await CreateAsync(user.Client, new CreateCategoryRequest("Pets", CategoryType.Expense, "#FF8A65", "pets"));

        var response = await user.Client.PutAsJsonAsync($"/api/categories/{pets.Id}",
            new UpdateCategoryRequest("Animais", "#123456", "pets"), TestJson.Options);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<CategoryResponse>(TestJson.Options);
        Assert.Equal("Animais", updated!.Name);
        Assert.Equal("#123456", updated.Color);
        Assert.Equal(CategoryType.Expense, updated.Type);
    }

    [Fact]
    public async Task Archive_HidesFromDefaultList()
    {
        var user = await TestUsers.CreateAsync(factory);
        var target = (await ListAsync(user.Client)).First(c => c.Name == "Lazer");

        await user.Client.PostAsync($"/api/categories/{target.Id}/archive", null);

        Assert.DoesNotContain(await ListAsync(user.Client), c => c.Id == target.Id);
        Assert.Contains(await ListAsync(user.Client, "?includeArchived=true"), c => c.Id == target.Id && c.IsArchived);
    }

    [Fact]
    public async Task Delete_RemovesCategory()
    {
        var user = await TestUsers.CreateAsync(factory);
        var target = (await ListAsync(user.Client)).First(c => c.Name == "Assinaturas");

        var response = await user.Client.DeleteAsync($"/api/categories/{target.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.DoesNotContain(await ListAsync(user.Client, "?includeArchived=true"), c => c.Id == target.Id);
    }
}
