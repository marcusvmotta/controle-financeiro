using System.ComponentModel.DataAnnotations;
using System.Text;

namespace FinanceControl.Infrastructure.Identity;

/// <summary>
/// Configuração dos tokens, lida da seção "Jwt" (appsettings ou variáveis Jwt__Key, Jwt__Issuer...).
/// As validações rodam quando a API sobe (ValidateOnStart): configuração errada impede a inicialização.
/// </summary>
public sealed class JwtOptions : IValidatableObject
{
    public const string SectionName = "Jwt";

    /// <summary>Chave secreta de assinatura (HMAC-SHA256). Mínimo de 32 bytes.</summary>
    [Required(ErrorMessage = "Jwt:Key é obrigatória. Defina a variável de ambiente Jwt__Key.")]
    public string Key { get; init; } = string.Empty;

    [Required]
    public string Issuer { get; init; } = "finance-control";

    [Required]
    public string Audience { get; init; } = "finance-control";

    [Range(1, 60)]
    public int AccessTokenMinutes { get; init; } = 15;

    [Range(1, 90)]
    public int RefreshTokenDays { get; init; } = 7;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Key) && Encoding.UTF8.GetByteCount(Key) < 32)
        {
            yield return new ValidationResult(
                "Jwt:Key precisa ter pelo menos 32 bytes (256 bits) para o HMAC-SHA256.",
                [nameof(Key)]);
        }
    }
}
