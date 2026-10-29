namespace FinanceControl.Domain.Common;

/// <summary>
/// "Nubank", " nubank " e "NUBANK" viram a mesma chave. Usada para impedir nomes repetidos
/// sem diferenciar maiúsculas de minúsculas (o mesmo truque do NormalizedEmail do Identity).
/// </summary>
public static class NameNormalizer
{
    public static string Normalize(string name) => name.Trim().ToUpperInvariant();
}
