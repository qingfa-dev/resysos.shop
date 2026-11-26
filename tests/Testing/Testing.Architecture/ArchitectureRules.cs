using System.Reflection;
using NetArchTest.Rules;

namespace Testing.Architecture;

/// <summary>Reusable layer and boundary rules applied to each module.</summary>
public static class ArchitectureRules
{
    /// <summary>Asserts that no type in an assembly depends on a forbidden namespace.</summary>
    /// <param name="assembly">The assembly to check.</param>
    /// <param name="forbidden">The forbidden namespace prefixes.</param>
    /// <returns>The NetArchTest result.</returns>
    public static TestResult NoDependency(Assembly assembly, params string[] forbidden)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(forbidden);

        return Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();
    }
}
