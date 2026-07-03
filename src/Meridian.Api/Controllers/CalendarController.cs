using System.IdentityModel.Tokens.Jwt;
using Meridian.Api.Authorization;
using Meridian.Bll.Dtos;
using Meridian.Bll.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

// Thin controller for the employee's own work calendar. The user id comes
// from the JWT sub claim — never from a route or query parameter.
[ApiController]
[Route("api/calendar")]
public class CalendarController : ControllerBase
{
    private readonly ICalendarService _calendar;

    public CalendarController(ICalendarService calendar)
    {
        _calendar = calendar;
    }

    [HttpGet("me")]
    [Authorize(Policy = Policies.CanRead)]
    [ProducesResponseType(typeof(CalendarMonthDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CalendarMonthDto>> GetMyMonth(
        [FromQuery] CalendarQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var userId))
        {
            return Unauthorized();
        }

        var today = DateTime.UtcNow;
        var month = await _calendar.GetMyMonthAsync(
            userId,
            query.Year ?? today.Year,
            query.Month ?? today.Month,
            cancellationToken);

        return Ok(month);
    }
}
