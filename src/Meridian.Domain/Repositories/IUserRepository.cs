using Meridian.Domain.Common;
using Meridian.Domain.Entities;

namespace Meridian.Domain.Repositories;

// Repository contract for users. Implemented in Dal.
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetByRoleAsync(Enums.Role role, CancellationToken cancellationToken = default);
}
