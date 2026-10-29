using FinanceControl.Application.Common.Validation;
using FinanceControl.Domain.Entities;
using FinanceControl.Domain.Enums;

namespace FinanceControl.UnitTests.Domain;

public sealed class AccountAndCategoryTests
{
    [Fact]
    public void AccountCreate_TrimsNameAndNormalizesForUniqueness()
    {
        var account = Account.Create(Guid.NewGuid(), "  Nubank ", AccountType.Checking, 10m, "#000000", "wallet");

        Assert.Equal("Nubank", account.Name);
        Assert.Equal("NUBANK", account.NormalizedName);
        Assert.False(account.IsArchived);
    }

    [Fact]
    public void AccountArchive_And_Unarchive()
    {
        var account = Account.Create(Guid.NewGuid(), "Nubank", AccountType.Checking, 10m, "#000000", "wallet");

        account.Archive();
        Assert.True(account.IsArchived);

        account.Unarchive();
        Assert.False(account.IsArchived);
    }

    [Fact]
    public void AccountCreate_WithBlankName_Throws()
    {
        Assert.ThrowsAny<ArgumentException>(() =>
            Account.Create(Guid.NewGuid(), "   ", AccountType.Cash, 0m, "#000000", "wallet"));
    }

    [Fact]
    public void CategoryUpdate_KeepsType()
    {
        var category = Category.Create(Guid.NewGuid(), "Pets", CategoryType.Expense, "#000000", "pets");

        category.Update("Animais", "#FFFFFF", "pets");

        Assert.Equal("Animais", category.Name);
        Assert.Equal(CategoryType.Expense, category.Type);
    }

    [Fact]
    public void DefaultCategories_HaveUniqueNamesPerType()
    {
        var duplicates = DefaultCategories.All
            .GroupBy(t => (t.Type, Name: t.Name.ToUpperInvariant()))
            .Where(g => g.Count() > 1);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void DefaultCategories_HaveBothTypes()
    {
        Assert.Contains(DefaultCategories.All, t => t.Type == CategoryType.Income);
        Assert.Contains(DefaultCategories.All, t => t.Type == CategoryType.Expense);
    }

    /// <summary>
    /// Os ícones das categorias padrão (Domain) precisam estar na lista permitida (Application).
    /// Senão, o usuário receberia uma categoria que não consegue editar: a validação recusaria o ícone.
    /// </summary>
    [Fact]
    public void DefaultCategories_UseOnlyAllowedIcons()
    {
        var notAllowed = DefaultCategories.All.Where(t => !AppearanceRules.AllowedIcons.Contains(t.Icon));

        Assert.Empty(notAllowed);
    }

    [Fact]
    public void DefaultCategories_CreateFor_AssignsTheUser()
    {
        var userId = Guid.NewGuid();

        var categories = DefaultCategories.CreateFor(userId).ToList();

        Assert.Equal(DefaultCategories.All.Count, categories.Count);
        Assert.All(categories, c => Assert.Equal(userId, c.UserId));
    }
}
