using DriveNow.ArchitectureTests.Support;
using FluentAssertions;
using NetArchTest.Rules;

namespace DriveNow.ArchitectureTests;

public class LayerDependencyTests
{
    // R1: DriveNow.Domain depends on no other DriveNow project.
    [Fact]
    [Trait("Rule", "ARCH-R1")]
    public void Domain_OtherDriveNowProjects_AreNotReferenced()
    {
        var result = Types.InAssembly(ProductionAssemblies.DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "DriveNow.Application",
                "DriveNow.Infrastructure",
                "DriveNow.ConsoleApp")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}", ProductionAssemblies.Describe(result));
    }

    // R2: DriveNow.Application depends only on DriveNow.Domain
    // (not on Infrastructure, not on ConsoleApp).
    [Fact]
    [Trait("Rule", "ARCH-R2")]
    public void Application_InfrastructureAndConsoleApp_AreNotReferenced()
    {
        var result = Types.InAssembly(ProductionAssemblies.ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "DriveNow.Infrastructure",
                "DriveNow.ConsoleApp")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}", ProductionAssemblies.Describe(result));
    }

    // R3: DriveNow.Infrastructure does not depend on DriveNow.ConsoleApp.
    [Fact]
    [Trait("Rule", "ARCH-R3")]
    public void Infrastructure_ConsoleApp_IsNotReferenced()
    {
        var result = Types.InAssembly(ProductionAssemblies.InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOn("DriveNow.ConsoleApp")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}", ProductionAssemblies.Describe(result));
    }
}