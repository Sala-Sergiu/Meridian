using Meridian.Domain.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Meridian.Dal.Decorators;

// Caching decorator (registered via Scrutor) wrapping ISampleRepository.
// Apply only to hot, rarely-changing data.
public class CachedSampleRepository : ISampleRepository
{
    private readonly ISampleRepository _inner;
    private readonly IMemoryCache _cache;

    public CachedSampleRepository(ISampleRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    // TODO: implement cached pass-through of ISampleRepository members per spec.
}
