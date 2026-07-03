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

    // The authenticated hire's own board cards, filtered/sorted/paged through
    // the query pipeline. The hire id comes from the JWT sub claim — never
    // from a route or query parameter.
    [HttpGet("me")]
    [Authorize(Policy = Policies.CanRead)]
    [ProducesResponseType(typeof(PagedResult<BoardCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<BoardCardDto>>> GetMyBoard(
        [FromQuery] BoardCardsQueryDto query,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var hireUserId))
        {
            return Unauthorized();
        }

        var page = await _boards.GetMyBoardCardsAsync(hireUserId, query, cancellationToken);
        return page is null ? NotFound() : Ok(page);
    }

    // The hire moves one of their OWN cards between columns — the only write
    // on the board. BoardOwnerWrite rejects read-only roles (HR/Manager) with
    // 403 at the authorization layer; the business layer then only ever
    // touches the caller's own board (id from the JWT sub claim, never from a
    // parameter), so a foreign or missing card is a plain 404.
    [HttpPatch("me/cards/{cardId:int}")]
    [Authorize(Policy = Policies.BoardOwnerWrite)]
    [ProducesResponseType(typeof(BoardCardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BoardCardDto>> MoveCard(
        int cardId,
        MoveCardRequestDto request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var hireUserId))
        {
            return Unauthorized();
        }

        var card = await _boards.MoveCardAsync(hireUserId, cardId, request, cancellationToken);
        return card is null ? NotFound() : Ok(card);
    }

    // HR/Manager tracking view: every new hire with their onboarding progress.
    // Static segment, so it never collides with the {hireUserId:int} route.
    [HttpGet("progress")]
    [Authorize(Policy = Policies.HrOrManagerRead)]
    [ProducesResponseType(typeof(IReadOnlyList<HireProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<HireProgressDto>>> GetProgress(CancellationToken cancellationToken)
    {
        return Ok(await _boards.GetHireProgressAsync(cancellationToken));
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
