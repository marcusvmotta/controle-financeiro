using FinanceControl.Application.Features.Accounts;
using FinanceControl.Domain.Enums;
using FluentValidation.TestHelper;

namespace FinanceControl.UnitTests.Application;

public sealed class AccountValidatorTests
{
    private readonly AccountRequestValidator _validator = new();

    private static AccountRequest Valid() => new("Nubank", AccountType.Checking, 100m, "#8A05BE", "account_balance");

    [Fact]
    public void Valid_HasNoErrors() => _validator.TestValidate(Valid()).ShouldNotHaveAnyValidationErrors();

    [Theory]
    [InlineData(10.5)]
    [InlineData(-320.10)]  // saldo negativo é permitido (cartão de crédito)
    [InlineData(0)]
    public void InitialBalance_WithUpTo2Decimals_IsValid(decimal balance) =>
        _validator.TestValidate(Valid() with { InitialBalance = balance })
            .ShouldNotHaveValidationErrorFor(x => x.InitialBalance);

    [Fact]
    public void InitialBalance_With3Decimals_IsInvalid() =>
        _validator.TestValidate(Valid() with { InitialBalance = 10.005m })
            .ShouldHaveValidationErrorFor(x => x.InitialBalance);

    [Theory]
    [InlineData("8A05BE")]
    [InlineData("#8A05B")]
    [InlineData("roxo")]
    public void Color_NotInHexFormat_IsInvalid(string color) =>
        _validator.TestValidate(Valid() with { Color = color }).ShouldHaveValidationErrorFor(x => x.Color);

    [Fact]
    public void Icon_NotInAllowedList_IsInvalid() =>
        _validator.TestValidate(Valid() with { Icon = "<script>" }).ShouldHaveValidationErrorFor(x => x.Icon);

    [Fact]
    public void Name_LongerThan60_IsInvalid() =>
        _validator.TestValidate(Valid() with { Name = new string('a', 61) }).ShouldHaveValidationErrorFor(x => x.Name);

    [Fact]
    public void Type_OutOfEnum_IsInvalid() =>
        _validator.TestValidate(Valid() with { Type = (AccountType)99 }).ShouldHaveValidationErrorFor(x => x.Type);
}
