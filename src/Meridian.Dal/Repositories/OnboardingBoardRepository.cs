using Meridian.Dal.Persistence;
using Meridian.Domain.Common;
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

    public async Task UpdateCardAsync(BoardCard card, CancellationToken cancellationToken = default)
    {
        // Cards are read untracked (AsNoTracking), so attach-and-mark-modified
        // before saving. DbContext is the unit of work.
        Context.Set<BoardCard>().Update(card);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedItems<BoardCard>?> GetBoardCardsAsync(
        int hireUserId,
        IReadOnlyList<IQueryStep<BoardCard>> steps,
        CancellationToken cancellationToken = default)
    {
        var boardId = await Context.Set<OnboardingBoard>()
            .AsNoTracking()
            .Where(b => b.HireUserId == hireUserId)
            .Select(b => (int?)b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (boardId is null)
        {
            return null;
        }

        var query = Context.Set<BoardCard>()
            .AsNoTracking()
            .Where(c => c.BoardId == boardId);

        foreach (var step in steps.Where(s => s is not IPagingStep<BoardCard>))
        {
            query = step.Apply(query);
        }

        // Count before paging so the total reflects the filtered-but-unpaged set.
        var totalCount = await query.CountAsync(cancellationToken);

        foreach (var step in steps.OfType<IPagingStep<BoardCard>>())
        {
            query = step.Apply(query);
        }

        var items = await query.ToListAsync(cancellationToken);
        return new PagedItems<BoardCard>(items, totalCount);
    }
}
