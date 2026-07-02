using Meridian.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace Meridian.Api.Authorization;

// Endpoint-level evaluation of BoardOwnerWrite, i.e. [Authorize(Policy =
// Policies.BoardOwnerWrite)] on self-scoped (/me) write routes where no
// explicit resource is supplied. Only new hires own boards, so the question
// answerable at this layer is the role: HR and Manager are read-only on hire
// progress and get 403 here — enforced by authorization, not hidden. Which
// card may be written is then decided in the business layer (the card must
// sit on the caller's own board).
// Guarded to HttpContext so this handler never satisfies resource-based
// checks — those stay with BoardOwnerAuthorizationHandler.
public class BoardOwnerEndpointHandler : AuthorizationHandler<BoardOwnerRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        BoardOwnerRequirement requirement)
    {
        if (context.Resource is HttpContext && context.User.IsInRole(nameof(Role.NewHire)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
