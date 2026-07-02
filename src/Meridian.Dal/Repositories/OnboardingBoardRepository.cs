using Meridian.Dal.Persistence;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Repositories;

// EF Core repository for per-hire onboarding boards. Reads load the board with
// its cards (ordered include); AddAsync saves directly — the DbContext is the
// unit of work.
public class OnboardingBoardRepository : RepositoryBase<OnboardingBoard>, IOnboardingBoardRepository
{
    public OnboardingBoardRepository(MeridianDbContext context)
        : base(context)
    {
    }

    public override async Task<OnboardingBoard?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<OnboardingBoard>()
            .AsNoTracking()
            .Include(b => b.Cards.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<OnboardingBoard>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<OnboardingBoard>()
            .AsNoTracking()
            .Include(b => b.Cards.OrderBy(c => c.Order))
            .ToListAsync(cancellationToken);
    }

    public async Task<OnboardingBoard?> GetByHireIdAsync(int hireUserId, CancellationToken cancellationToken = default)
    {
        return await Context.Set<OnboardingBoard>()
            .AsNoTracking()
            .Include(b => b.Cards.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(b => b.HireUserId == hireUserId, cancellationToken);
    }

    public async Task AddAsync(OnboardingBoard board, CancellationToken cancellationToken = default)
    {
        Context.Set<OnboardingBoard>().Add(board);
        await Context.SaveChangesAsync(cancellationToken);
    }
}
