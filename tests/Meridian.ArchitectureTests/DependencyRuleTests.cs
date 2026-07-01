using NetArchTest.Rules;

namespace Meridian.ArchitectureTests;

// NetArchTest rules enforcing the dependency directions from CLAUDE.md:
//  - Meridian.Domain depends on nothing (no EF Core, no other project).
//  - Meridian.Bll must not reference Meridian.Dal or EF Core.
//  - Meridian.Api must not reference Meridian.Dal except in Program.cs (composition root).
//  - Repository interfaces in Domain; implementations in Dal.
//  - Dependencies always point inward (toward Domain).
public class DependencyRuleTests
{
    // TODO: implement each rule above as a [Fact] using Types.InAssembly(...).
}
