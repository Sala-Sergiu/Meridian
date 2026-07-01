using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Meridian.Api.Authorization;
using Meridian.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    // Any authenticated user may read (managers included). Proves auth end to end.
    [HttpGet("me")]
    [Authorize(Policy = Policies.CanRead)]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<CurrentUserResponse> Me()
    {
        var id = int.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out var parsed) ? parsed : 0;
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email) ?? string.Empty;
        var displayName = User.FindFirstValue("name") ?? string.Empty;
        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        return Ok(new CurrentUserResponse(id, email, displayName, role));
    }
}
