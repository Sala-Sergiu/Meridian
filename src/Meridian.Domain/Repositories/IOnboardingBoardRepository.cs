using Meridian.Domain.Common;
using Meridian.Domain.Entities;

namespace Meridian.Domain.Repositories;

// Repository contract for per-hire onboarding boards. Implemented in Dal,
// where reads load the board together with its cards. Boards are per-hire and
// change as the hire progresses — no caching decorator here.
public interface IOnboardingBoardRepository : IRepository<OnboardingBoard>
{
    // A hire has at most one board.
    Task<OnboardingBoard?> GetByHireIdAsync(int hireUserId, CancellationToken cancellationToken = default);

    // Persists immediately — the DbContext is the unit of work.
    Task AddAsync(OnboardingBoard board, CancellationToken cancellationToken = default);

    // Persists changes to an existing board card immediately — the DbContext
    // is the unit of work; no IUnitOfWork.
    Task UpdateCardAsync(BoardCard card, CancellationToken cancellationToken = default);

    // Bulk-adds cards (one per board) in a single save — used when HR
    // publishes a new article to every existing board.
    Task AddCardsAsync(IReadOnlyList<BoardCard> cards, CancellationToken cancellationToken = default);

    // Runs the composed query pipeline over the hire's board cards and returns
    // materialized results — IQueryable never escapes Dal. TotalCount is
    // computed before any IPagingStep is applied. Null when the hire has no
    // board (distinct from a board whose cards all got filtered out).
    Task<PagedItems<BoardCard>?> GetBoardCardsAsync(
        int hireUserId,
        IReadOnlyList<IQueryStep<BoardCard>> steps,
        CancellationToken cancellationToken = default);
}
