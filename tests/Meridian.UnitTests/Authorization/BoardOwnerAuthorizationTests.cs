using System.Security.Claims;
using Meridian.Api.Authorization;
using Meridian.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Meridian.UnitTests.Authorization;

// The owner-only rule declared in the auth slice, now under test:
// - resource-based: only the owner of the resource passes;
// - endpoint-level (/me write routes): only the NewHire role passes, so
//   HR/Manager are rejected by the authorization layer, not hidden.
public class BoardOwnerAuthorizationTests
{
    private sealed record TestBoardResource(int OwnerUserId) : IResourceOwner;

    private static ClaimsPrincipal UserWith(int id, Role role) => new(new ClaimsIdentity(
        new[]
        {
            new Claim("sub", id.ToString()),
            new Claim(ClaimTypes.Role, role.ToString())
        },
        authenticationType: "test"));

    private static AuthorizationHandlerContext Context(ClaimsPrincipal user, object? resource)
        => new(new[] { new BoardOwnerRequirement() }, user, resource);

    [Fact]
    public async Task ResourceHandler_Owner_Succeeds()
    {
        var context = Context(UserWith(42, Role.NewHire), new TestBoardResource(42));

        await new BoardOwnerAuthorizationHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceHandler_DifferentUser_DoesNotSucceed()
    {
        var context = Context(UserWith(43, Role.NewHire), new TestBoardResource(42));

        await new BoardOwnerAuthorizationHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task EndpointHandler_NewHireOnEndpoint_Succeeds()
    {
        var context = Context(UserWith(42, Role.NewHire), new DefaultHttpContext());

        await new BoardOwnerEndpointHandler().HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [InlineData(Role.HR)]
    [InlineData(Role.Manager)]
    public async Task EndpointHandler_ReadOnlyRoles_DoNotSucceed(Role role)
    {
        var context = Context(UserWith(2, role), new DefaultHttpContext());

        await new BoardOwnerEndpointHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task EndpointHandler_NeverSatisfiesResourceBasedChecks()
    {
        // A NewHire checked against ANOTHER hire's board resource must not be
        // let through by the endpoint-level handler.
        var context = Context(UserWith(43, Role.NewHire), new TestBoardResource(42));

        await new BoardOwnerEndpointHandler().HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }
}
