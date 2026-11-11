using NetArchTest.Rules;
using ParkSmart.Application;
using ParkSmart.Domain;
using System.Reflection;

namespace ParkSmart.ArchitectureTests;

public class ArchitectureTests
{
    private static readonly Assembly DomainAssembly = typeof(ParkingSession).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(ParkingFeeCalculator).Assembly;

    [Fact]
    public void Domain_Should_Not_Have_Dependency_On_Other_Projects()
    {
        var result = Types.InAssembly(DomainAssembly)
            .Should()
            .NotHaveDependencyOnAny("ParkSmart.Application", "ParkSmart.Infrastructure", "ParkSmart.ConsoleApp")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_Should_Not_Have_Dependency_On_Infrastructure_Or_ConsoleApp()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOnAny("ParkSmart.Infrastructure", "ParkSmart.ConsoleApp")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Interfaces_Should_Start_With_I()
    {
        var result = Types.InAssembly(ApplicationAssembly)
            .That().AreInterfaces()
            .Should().HaveNameStartingWith("I")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}