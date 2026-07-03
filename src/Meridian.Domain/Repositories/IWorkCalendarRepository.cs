using Meridian.Domain.Entities;

namespace Meridian.Domain.Repositories;

// Read access to the data behind an employee's work calendar: their weekly
// office/remote pattern and the public holidays in a date range.
public interface IWorkCalendarRepository
{
    Task<EmployeeSchedule?> GetScheduleByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PublicHoliday>> GetHolidaysAsync(
        DateTime fromInclusive,
        DateTime toExclusive,
        CancellationToken cancellationToken = default);
}
