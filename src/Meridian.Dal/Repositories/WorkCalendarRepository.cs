using Meridian.Dal.Persistence;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Repositories;

// Read-only lookups for the work calendar; nothing here is tracked.
public class WorkCalendarRepository : IWorkCalendarRepository
{
    private readonly MeridianDbContext _context;

    public WorkCalendarRepository(MeridianDbContext context)
    {
        _context = context;
    }

    public async Task<EmployeeSchedule?> GetScheduleByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.EmployeeSchedules
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<PublicHoliday>> GetHolidaysAsync(
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default)
    {
        return await _context.PublicHolidays
            .AsNoTracking()
            .Where(h => h.Date >= fromInclusive && h.Date < toExclusive)
            .OrderBy(h => h.Date)
            .ToListAsync(cancellationToken);
    }
}
