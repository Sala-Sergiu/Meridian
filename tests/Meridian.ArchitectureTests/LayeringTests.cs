using NetArchTest.Rules;

namespace Meridian.ArchitectureTests;

// Enforces the CLAUDE.md dependency rules. The key assertion of the query
// pipeline slice: Bll composes IQueryable steps yet stays free of EF Core —
// the steps are pure System.Linq.
public class LayeringTests
{
    private static readonly System.Reflection.Assembly DomainAssembly =
        typeof(Meridian.Domain.Common.BaseEntity).Assembly;

    private static readonly System.Reflection.Assembly BllAssembly =
        typeof(Meridian.Bll.DependencyInjection).Assembly;

    private static string Failures(TestResult result)
        => string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>());

    [Fact]
    public void Bll_DoesNotDependOn_EfCore()
    {
        var result = Types.InAssembly(BllAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, $"BLL types depending on EF Core: {Failures(result)}");
    }

    [Fact]
    public void Bll_DoesNotDependOn_Dal()
    {
        var result = Types.InAssembly(BllAssembly)
            .ShouldNot()
            .HaveDependencyOn("Meridian.Dal")
            .GetResult();

        Assert.True(result.IsSuccessful, $"BLL types depending on Dal: {Failures(result)}");
    }

    [Fact]
    public void Domain_DependsOnNothing_OutsideItself()
    {
        var result = Types.InAssembly(DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Meridian.Bll",
                "Meridian.Dal",
                "Meridian.Api",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, $"Domain types with outward dependencies: {Failures(result)}");
    }
}
