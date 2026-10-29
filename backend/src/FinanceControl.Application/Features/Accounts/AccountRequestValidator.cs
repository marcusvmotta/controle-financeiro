using FinanceControl.Application.Common.Validation;
using FluentValidation;

namespace FinanceControl.Application.Features.Accounts;

public sealed class AccountRequestValidator : AbstractValidator<AccountRequest>
{
    private const decimal MaxAbsoluteBalance = 999_999_999.99m;

    public AccountRequestValidator()
    {
        RuleFor(x => x.Name).ValidName(maxLength: 60);

        RuleFor(x => x.Type).IsInEnum().WithMessage("Tipo de conta inválido.");

        RuleFor(x => x.InitialBalance)
            .InclusiveBetween(-MaxAbsoluteBalance, MaxAbsoluteBalance)
            .WithMessage("O saldo inicial deve estar entre -999.999.999,99 e 999.999.999,99.")
            // Dinheiro tem no máximo 2 casas decimais (ADR-012). 10,005 não é um valor válido.
            .PrecisionScale(precision: 18, scale: 2, ignoreTrailingZeros: true)
            .WithMessage("O saldo inicial deve ter no máximo 2 casas decimais.");

        RuleFor(x => x.Color).ValidColor();
        RuleFor(x => x.Icon).ValidIcon();
    }
}
