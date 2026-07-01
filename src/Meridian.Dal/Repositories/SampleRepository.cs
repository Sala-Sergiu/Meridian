using Meridian.Dal.Persistence;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;

namespace Meridian.Dal.Repositories;

// Concrete EF Core repository for SampleEntity.
public class SampleRepository : RepositoryBase<SampleEntity>, ISampleRepository
{
    public SampleRepository(MeridianDbContext context)
        : base(context)
    {
    }

    // TODO: implement ISampleRepository members per spec.
}
