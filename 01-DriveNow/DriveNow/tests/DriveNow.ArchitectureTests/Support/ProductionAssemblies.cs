using DriveNow.Application.Services;
using DriveNow.Domain.Services;
using DriveNow.Infrastructure.Persistence;
using NetArchTest.Rules;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DriveNow.ArchitectureTests.Support;

/// <summary>
/// Gives the architecture tests the DriveNow production assemblies
/// and a few helpers shared by all rules.
/// </summary>
internal static class ProductionAssemblies
{
    // Each assembly is found through one public type that lives in it.
    public static readonly Assembly DomainAssembly = typeof(RentalMath).Assembly;
    public static readonly Assembly ApplicationAssembly = typeof(BookingService).Assembly;
    public static readonly Assembly InfrastructureAssembly = typeof(InMemoryDatabase).Assembly;

    // Program is internal, so ConsoleApp is loaded by name.
    public static readonly Assembly ConsoleAppAssembly = Assembly.Load("DriveNow.ConsoleApp");

    // The three class libraries (ConsoleApp is only the staff menu).
    public static readonly Assembly[] Libraries =
        [DomainAssembly, ApplicationAssembly, InfrastructureAssembly];

    /// <summary>All top-level, hand-written classes of the three libraries.</summary>
    public static IEnumerable<Type> ProductionClasses() =>
        Types.InAssemblies(Libraries)
            .GetTypes()
            .Where(t => t.IsClass
                        && !t.IsNested
                        && t.GetCustomAttribute<CompilerGeneratedAttribute>() is null);

    /// <summary>True when the type lives in a namespace whose last part is the given name.</summary>
    public static bool InNamespaceNamed(Type type, string lastPart) =>
        type.Namespace is not null
        && (type.Namespace == lastPart || type.Namespace.EndsWith("." + lastPart));

    /// <summary>Lists the types that failed a NetArchTest rule, for the assertion message.</summary>
    public static string Describe(TestResult result) =>
        string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());
}