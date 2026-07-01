using Meridian.Dal.Persistence;
using Meridian.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Repositories;

// Generic EF Core repository base. EF-specific calls (AsNoTracking, async
// materializers) live here — IQueryable never escapes this layer.
public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    protected readonly MeridianDbContext Context;

    protected RepositoryBase(MeridianDbContext context)
    {
        Context = context;
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>()
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Set<T>()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
