namespace Meridian.Domain.Common;

// Generic repository contract. Lives in Domain; implemented in Dal.
// Thin on purpose — specific repositories add members only when they earn
// their place (extra queries, caching seam).
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);
}
