using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FinanceControl.Application;

public static class DependencyInjection
{
    /// <summary>Registra os serviços da camada Application (validators e, a partir do M2, os services de cada feature).</summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Encontra e registra todos os AbstractValidator<T> deste projeto.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }
}
