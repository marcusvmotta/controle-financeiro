namespace FinanceControl.Application.Abstractions;

/// <summary>
/// Quem está fazendo a requisição. Lido do token JWT pela camada Api.
/// O UserId vem SEMPRE daqui, nunca do corpo da requisição (TRD 04-seguranca).
/// </summary>
public interface ICurrentUser
{
    /// <summary>Id do usuário autenticado. Lança UnauthorizedException se não houver usuário.</summary>
    Guid UserId { get; }

    bool IsAuthenticated { get; }
}
