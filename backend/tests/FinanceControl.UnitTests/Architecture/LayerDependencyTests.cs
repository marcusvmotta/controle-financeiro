using System.Reflection;

namespace FinanceControl.UnitTests.Architecture;

/// <summary>
/// Garante as regras de dependência entre camadas do ADR-001.
/// Se alguém adicionar uma referência proibida, este teste quebra no CI.
/// </summary>
public sealed class LayerDependencyTests
{
    [Theory]
    [InlineData("FinanceControl.Application")]
    [InlineData("FinanceControl.Infrastructure")]
    [InlineData("FinanceControl.Api")]
    [InlineData("Microsoft.EntityFrameworkCore")]
    [InlineData("Microsoft.AspNetCore")]
    public void Domain_DoesNotDependOn(string forbiddenAssembly)
    {
        var references = ReferencedAssemblyNames("FinanceControl.Domain");

        Assert.DoesNotContain(references, name => name.StartsWith(forbiddenAssembly, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("FinanceControl.Infrastructure")]
    [InlineData("FinanceControl.Api")]
    public void Application_DoesNotDependOn(string forbiddenAssembly)
    {
        var references = ReferencedAssemblyNames("FinanceControl.Application");

        Assert.DoesNotContain(references, name => name.StartsWith(forbiddenAssembly, StringComparison.Ordinal));
    }

    private static IEnumerable<string> ReferencedAssemblyNames(string assemblyName) =>
        Assembly.Load(assemblyName)
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty);
}
