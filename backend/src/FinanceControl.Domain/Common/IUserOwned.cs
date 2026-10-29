namespace FinanceControl.Domain.Common;

/// <summary>
/// Marca entidades que pertencem a um usuário. Toda entidade com esta interface recebe
/// um filtro global no EF Core: as consultas só enxergam os registros do usuário logado (TRD 04-seguranca).
/// </summary>
public interface IUserOwned
{
    Guid UserId { get; }
}

/// <summary>Entidades com data de criação e de última alteração, preenchidas automaticamente ao salvar.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    DateTimeOffset UpdatedAt { get; }
}
