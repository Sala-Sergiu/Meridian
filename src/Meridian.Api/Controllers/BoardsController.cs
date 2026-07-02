using System.IdentityModel.Tokens.Jwt;
using Meridian.Api.Authorization;
using Meridian.Bll.Dtos;
using Meridian.Bll.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meridian.Api.Controllers;

// Thin controller for per-hire onboarding boards. Resource-aware access:
// only HR assigns; a hire reads only their OWN board (id from the JWT, never
// from the route); HR/Manager read any hire's board.
[ApiController]
[Route("api/boards")]
public class BoardsController : ControllerBase
{
    private readonly IOnboardingBoardService _boards;

    public BoardsController(IOnboardingBoardService boards)
    {
        _boards = boards;
    }

    // HR assigns onboarding: clones the template into the hire's board.
    [HttpPost("assign")]
    [Authorize(Policy = Policies.HrWrite)]
    [ProducesResponseType(typeof(OnboardingBoardDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(OnboardingBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OnboardingBoardDto>> Assign(
        AssignBoardRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _boards.AssignAsync(request.TemplateId, request.HireUserId, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        // Idempotent repeat: the hire already had a board — return it, no duplicate.
        return result.AlreadyExisted
            ? Ok(result.Board)
            : CreatedAtAction(nameof(GetBoard), new { hireUserId = request.HireUserId }, result.Board);
    }

    // The authenticated hire's own board. The hire id comes from the JWT sub
    // claim — never from a route or query parameter.
    [HttpGet("me")]
    [Authorize(Policy = Policies.CanRead)]
    [ProducesResponseType(typeof(OnboardingBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OnboardingBoardDto>> GetMyBoard(CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var hireUserId))
        {
            return Unauthorized();
        }

        var board = await _boards.GetMyBoardAsync(hireUserId, cancellationToken);
        return board is null ? NotFound() : Ok(board);
    }

    // HR/Manager progress view over any hire's board. A NewHire never satisfies
    // this policy, so they cannot read another hire's board through this route.
    [HttpGet("{hireUserId:int}")]
    [Authorize(Policy = Policies.HrOrManagerRead)]
    [ProducesResponseType(typeof(OnboardingBoardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OnboardingBoardDto>> GetBoard(int hireUserId, CancellationToken cancellationToken)
    {
        var board = await _boards.GetMyBoardAsync(hireUserId, cancellationToken);
        return board is null ? NotFound() : Ok(board);
    }
}
