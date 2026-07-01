namespace Meridian.Domain.Common;

// Generic repository contract. Lives in Domain; implemented in Dal.
// CRUD / query members to be defined per spec.
public interface IRepository<T> where T : BaseEntity
{
}
