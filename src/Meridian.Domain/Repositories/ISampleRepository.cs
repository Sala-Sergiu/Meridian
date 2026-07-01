using Meridian.Domain.Common;
using Meridian.Domain.Entities;

namespace Meridian.Domain.Repositories;

// Specific repository contract for SampleEntity.
// Add only members that earn their place (extra queries, caching seam).
public interface ISampleRepository : IRepository<SampleEntity>
{
}
