using FinanceControl.Application.Common.Validation;
using FluentValidation;

namespace FinanceControl.Application.Features.Categories;

public sealed class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).ValidName(maxLength: 40);
        RuleFor(x => x.Type).IsInEnum().WithMessage("Tipo de categoria inválido.");
        RuleFor(x => x.Color).ValidColor();
        RuleFor(x => x.Icon).ValidIcon();
    }
}

public sealed class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).ValidName(maxLength: 40);
        RuleFor(x => x.Color).ValidColor();
        RuleFor(x => x.Icon).ValidIcon();
    }
}
