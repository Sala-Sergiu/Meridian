using Meridian.Bll.Dtos;

namespace Meridian.Bll.Services;

// The employee's own work calendar: office/remote pattern + public holidays.
public interface ICalendarService
{
    // Classifies every day of the month for this user. An employee without a
    // stored schedule is treated as fully remote (deliberate default) — the
    // month still renders with weekends and holidays.
    Task<CalendarMonthDto> GetMyMonthAsync(int userId, int year, int month, CancellationToken cancellationToken = default);
}
