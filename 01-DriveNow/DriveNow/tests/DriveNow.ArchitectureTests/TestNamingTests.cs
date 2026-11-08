using DriveNow.UnitTests;
using FluentAssertions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DriveNow.ArchitectureTests;

public class TestNamingTests
{
    // Method_Scenario_ExpectedResult: exactly three parts, no underscore inside a part.
    private static readonly Regex MethodNamePattern =
        new("^[A-Za-z0-9]+_[A-Za-z0-9]+_[A-Za-z0-9]+$");

    // NetArchTest only checks types, so test methods are found with reflection.
    // [Theory] counts too, because TheoryAttribute inherits from FactAttribute.
    private static IEnumerable<MethodInfo> UnitTestMethods() =>
        typeof(RentalMathTests).Assembly
            .GetTypes()
            .SelectMany(type => type.GetMethods(
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.Static |
                BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttributes(typeof(FactAttribute), inherit: true).Any());

    // Test classes end with Tests.
    [Fact]
    [Trait("Rule", "ARCH-R5")]
    public void TestClasses_AllUnitTestClasses_EndWithTests()
    {
        var offenders = UnitTestMethods()
            .Select(method => method.DeclaringType!)
            .Distinct()
            .Where(type => !type.Name.EndsWith("Tests"))
            .Select(type => type.FullName)
            .ToList();

        offenders.Should().BeEmpty("test classes must end with 'Tests'");
    }

    // Every [Fact] / [Theory] method matches the naming pattern.
    [Fact]
    [Trait("Rule", "ARCH-R5")]
    public void TestMethods_AllUnitTestMethods_MatchNamingPattern()
    {
        var offenders = UnitTestMethods()
            .Where(method => !MethodNamePattern.IsMatch(method.Name))
            .Select(method => $"{method.DeclaringType!.Name}.{method.Name}")
            .ToList();

        offenders.Should().BeEmpty("test methods must follow Method_Scenario_ExpectedResult");
    }
}