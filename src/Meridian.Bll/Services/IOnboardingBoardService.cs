using Meridian.Bll.Dtos;

namespace Meridian.Bll.Services;

// Use-cases for per-hire onboarding boards: assignment (template cloning) and
// board reads.
public interface IOnboardingBoardService
{
    // Clones the template into a new board for the hire. Returns null when the
    // template does not exist. Idempotent: if the hire already has a board it
    // is returned unchanged with AlreadyExisted = true — never a duplicate.
    Task<AssignBoardResultDto?> AssignAsync(int templateId, int hireUserId, CancellationToken cancellationToken = default);

    // The hire's own board (or any hire's board when called by HR/Manager —
    // authorization happens at the API layer). Null when no board is assigned.
    Task<OnboardingBoardDto?> GetMyBoardAsync(int hireUserId, CancellationToken cancellationToken = default);

    // The hire's board cards filtered/sorted/paged through the query pipeline.
    // Null when the hire has no board (as opposed to an empty page).
    Task<PagedResult<BoardCardDto>?> GetMyBoardCardsAsync(
        int hireUserId,
        BoardCardsQueryDto query,
        CancellationToken cancellationToken = default);
}
