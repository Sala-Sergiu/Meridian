using Meridian.Dal.Persistence;
using Meridian.Domain.Common;

namespace Meridian.Dal.Repositories;

// Generic EF Core repository base. Runs query-pipeline steps against DbSet<T>
// and materializes results — IQueryable never escapes this layer.
public abstract class RepositoryBase<T> : IRepository<T> where T : BaseEntity
{
    protected readonly MeridianDbContext Context;

    protected RepositoryBase(MeridianDbContext context)
    {
        Context = context;
    }

    // TODO: implement shared CRUD / materialization helpers per spec.
}
