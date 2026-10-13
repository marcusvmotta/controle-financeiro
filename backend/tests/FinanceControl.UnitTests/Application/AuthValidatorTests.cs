using FinanceControl.Application.Features.Auth;
using FluentValidation.TestHelper;

namespace FinanceControl.UnitTests.Application;

/// <summary>TestValidate (do FluentValidation.TestHelper) facilita checar em qual campo está o erro.</summary>
public sealed class AuthValidatorTests
{
    private readonly RegisterRequestValidator _register = new();
    private readonly LoginRequestValidator _login = new();

    [Fact]
    public void Register_ValidRequest_HasNoErrors()
    {
        var result = _register.TestValidate(new RegisterRequest("Ana Souza", "ana@email.com", "senha1234"));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("   ")]
    public void Register_InvalidName_HasNameError(string name)
    {
        var result = _register.TestValidate(new RegisterRequest(name, "ana@email.com", "senha1234"));

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("nao-e-email")]
    public void Register_InvalidEmail_HasEmailError(string email)
    {
        var result = _register.TestValidate(new RegisterRequest("Ana Souza", email, "senha1234"));

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("curta1", "entre 8 e 128")]
    [InlineData("semnumeros", "uma letra e um número")]
    [InlineData("12345678", "uma letra e um número")]
    public void Register_WeakPassword_HasPasswordErrorWithMessage(string password, string expectedMessagePart)
    {
        var result = _register.TestValidate(new RegisterRequest("Ana Souza", "ana@email.com", password));

        result.ShouldHaveValidationErrorFor(x => x.Password);
        Assert.Contains(result.Errors, error =>
            error.PropertyName == nameof(RegisterRequest.Password) &&
            error.ErrorMessage.Contains(expectedMessagePart, StringComparison.Ordinal));
    }

    [Fact]
    public void Register_NullFields_ReportsErrorsWithoutThrowing()
    {
        // Um JSON sem os campos chega como null; a validação precisa responder com erro, não com exceção.
        var result = _register.TestValidate(new RegisterRequest(null!, null!, null!));

        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Login_EmptyPassword_HasPasswordError()
    {
        var result = _login.TestValidate(new LoginRequest("ana@email.com", ""));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
