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
}
