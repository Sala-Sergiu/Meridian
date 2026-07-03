using Meridian.Dal.Persistence;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Repositories;

// EF Core repository for users. EF-specific calls (AsNoTracking, async
// materializers) live here in Dal — IQueryable never escapes this layer.
public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(MeridianDbContext context)
        : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetByRoleAsync(Domain.Enums.Role role, CancellationToken cancellationToken = default)
    {
        return await Context.Set<User>()
            .AsNoTracking()
            .Where(u => u.Role == role)
            .OrderBy(u => u.DisplayName)
            .ToListAsync(cancellationToken);
    }
}
