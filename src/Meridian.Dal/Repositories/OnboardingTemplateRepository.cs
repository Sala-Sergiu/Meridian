using Meridian.Dal.Persistence;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Repositories;

// EF Core repository for onboarding templates. Overrides the generic reads to
// load cards with the template (ordered include) — EF-specific calls stay here.
public class OnboardingTemplateRepository : RepositoryBase<OnboardingTemplate>, IOnboardingTemplateRepository
{
    public OnboardingTemplateRepository(MeridianDbContext context)
        : base(context)
    {
    }

    public override async Task<OnboardingTemplate?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<OnboardingTemplate>()
            .AsNoTracking()
            .Include(t => t.Cards.OrderBy(c => c.Order))
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public override async Task<IReadOnlyList<OnboardingTemplate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<OnboardingTemplate>()
            .AsNoTracking()
            .Include(t => t.Cards.OrderBy(c => c.Order))
            .ToListAsync(cancellationToken);
    }
}
