using FluentValidation;

namespace FinanceControl.Application.Common.Validation;

/// <summary>
/// Regras de cor e ícone, compartilhadas por contas e categorias (TRD modulos/02-contas.md, "Validações").
/// </summary>
public static class AppearanceRules
{
    /// <summary>
    /// Ícones permitidos (nomes do Material Symbols). Uma lista fechada evita que qualquer texto
    /// arbitrário seja gravado e depois exibido na tela.
    /// </summary>
    public static IReadOnlySet<string> AllowedIcons { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        // Contas
        "account_balance", "account_balance_wallet", "savings", "credit_card", "payments", "wallet",
        "currency_exchange", "attach_money", "trending_up",
        // Categorias
        "restaurant", "local_cafe", "home", "directions_car", "local_gas_station", "directions_bus",
        "medical_services", "fitness_center", "sports_esports", "movie", "school", "menu_book",
        "shopping_bag", "shopping_cart", "subscriptions", "pets", "child_care", "checkroom",
        "flight", "work", "card_giftcard", "receipt_long", "bolt", "water_drop", "wifi",
        "phone_iphone", "more_horiz",
    };

    public static IRuleBuilderOptions<T, string> ValidColor<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("A cor é obrigatória.")
            .Matches("^#[0-9A-Fa-f]{6}$").WithMessage("A cor deve estar no formato #RRGGBB.");

    public static IRuleBuilderOptions<T, string> ValidIcon<T>(this IRuleBuilderInitial<T, string> rule) =>
        rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O ícone é obrigatório.")
            .Must(icon => AllowedIcons.Contains(icon)).WithMessage("Ícone não permitido.");

    /// <summary>Nome obrigatório, sem contar espaços nas pontas, com tamanho máximo.</summary>
    public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilderInitial<T, string> rule, int maxLength) =>
        rule
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .Must(name => name.Trim().Length is > 0 && name.Trim().Length <= maxLength)
            .WithMessage($"O nome deve ter entre 1 e {maxLength} caracteres.");
}
