using FluentValidation;

namespace FinanceControl.Application.Features.Auth;

// Regras do TRD (modulos/01-autenticacao.md). As mensagens vão direto para a tela, por isso em português.

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .Must(name => name.Trim().Length is >= 2 and <= 100)
            .WithMessage("O nome deve ter entre 2 e 100 caracteres.");

        RuleFor(x => x.Email).ValidEmail();

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A senha é obrigatória.")
            .Length(8, 128).WithMessage("A senha deve ter entre 8 e 128 caracteres.")
            .Must(password => password.Any(char.IsLetter) && password.Any(char.IsDigit))
            .WithMessage("A senha deve ter pelo menos uma letra e um número.");
    }
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).ValidEmail();
        RuleFor(x => x.Password).NotEmpty().WithMessage("A senha é obrigatória.");
    }
}

internal static class AuthValidationRules
{
    /// <summary>Regra de e-mail reutilizada no cadastro e no login.</summary>
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O e-mail é obrigatório.")
            .MaximumLength(254).WithMessage("O e-mail deve ter no máximo 254 caracteres.")
            .EmailAddress().WithMessage("Informe um e-mail válido.");
}
