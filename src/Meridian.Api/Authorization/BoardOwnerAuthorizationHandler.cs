using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Meridian.Api.Authorization;

// Resource-based handler: succeeds only when the authenticated user is the
// owner of the resource being accessed. Write endpoints in later slices call
// IAuthorizationService.AuthorizeAsync(user, resource, Policies.BoardOwnerWrite).
public class BoardOwnerAuthorizationHandler
    : AuthorizationHandler<BoardOwnerRequirement, IResourceOwner>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BoardOwnerRequirement requirement,
        IResourceOwner resource)
    {
        var subject = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                      ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (int.TryParse(subject, out var userId) && userId == resource.OwnerUserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
