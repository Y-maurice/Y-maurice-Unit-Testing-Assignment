using DriveNow.ArchitectureTests.Support;
using FluentAssertions;
using NetArchTest.Rules;

namespace DriveNow.ArchitectureTests;

public class ProductionNamingTests
{
    // Interfaces start with I.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Interfaces_AllProductionTypes_StartWithI()
    {
        var result = Types.InAssemblies(ProductionAssemblies.Libraries)
            .That().AreInterfaces()
            .Should().HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}", ProductionAssemblies.Describe(result));
    }

    // Classes that live in a Repositories namespace end with Repository.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Repositories_TypeNames_EndWithRepository()
    {
        var offenders = ProductionAssemblies.ProductionClasses()
            .Where(t => ProductionAssemblies.InNamespaceNamed(t, "Repositories"))
            .Where(t => !t.Name.EndsWith("Repository"))
            .Select(t => t.FullName)
            .ToList();

        offenders.Should().BeEmpty("these types break the rule: {0}", string.Join(", ", offenders));
    }

    // Classes whose name ends with Repository live in a Repositories namespace.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Repositories_Namespace_IsRepositories()
    {
        var offenders = ProductionAssemblies.ProductionClasses()
            .Where(t => t.Name.EndsWith("Repository"))
            .Where(t => !ProductionAssemblies.InNamespaceNamed(t, "Repositories"))
            .Select(t => t.FullName)
            .ToList();

        offenders.Should().BeEmpty("these types break the rule: {0}", string.Join(", ", offenders));
    }

    // Classes that live in a Services namespace end with Service.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Services_TypeNames_EndWithService()
    {
        var offenders = ProductionAssemblies.ProductionClasses()
            .Where(t => ProductionAssemblies.InNamespaceNamed(t, "Services"))
            .Where(t => !t.Name.EndsWith("Service"))
            .Select(t => t.FullName)
            .ToList();

        offenders.Should().BeEmpty("these types break the rule: {0}", string.Join(", ", offenders));
    }

    // Classes whose name ends with Service live in a Services namespace.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Services_Namespace_IsServices()
    {
        var offenders = ProductionAssemblies.ProductionClasses()
            .Where(t => t.Name.EndsWith("Service"))
            .Where(t => !ProductionAssemblies.InNamespaceNamed(t, "Services"))
            .Select(t => t.FullName)
            .ToList();

        offenders.Should().BeEmpty("these types break the rule: {0}", string.Join(", ", offenders));
    }

    // Every exception class ends with Exception.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Exceptions_TypeNames_EndWithException()
    {
        var result = Types.InAssemblies(ProductionAssemblies.Libraries)
            .That().Inherit(typeof(Exception))
            .Should().HaveNameEndingWith("Exception")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}", ProductionAssemblies.Describe(result));
    }

    // Every class whose name ends with Exception lives in DriveNow.Domain.Exceptions.
    [Fact]
    [Trait("Rule", "ARCH-R4")]
    public void Exceptions_Namespace_IsDomainExceptions()
    {
        var result = Types.InAssemblies(ProductionAssemblies.Libraries)
            .That().HaveNameEndingWith("Exception")
            .Should().ResideInNamespace("DriveNow.Domain.Exceptions")
            .GetResult();

        result.IsSuccessful.Should().BeTrue(
            "these types break the rule: {0}", ProductionAssemblies.Describe(result));
    }
}