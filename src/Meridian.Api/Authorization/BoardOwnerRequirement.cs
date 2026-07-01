using Microsoft.AspNetCore.Authorization;

namespace Meridian.Api.Authorization;

// Requirement for the BoardOwnerWrite policy: the caller must own the resource.
public class BoardOwnerRequirement : IAuthorizationRequirement
{
}
