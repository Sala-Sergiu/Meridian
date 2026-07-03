using Meridian.Bll.Dtos;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;

namespace Meridian.Bll.Services;

public class CalendarService : ICalendarService
{
    private readonly IWorkCalendarRepository _calendar;

    public CalendarService(IWorkCalendarRepository calendar)
    {
        _calendar = calendar;
    }

    public async Task<CalendarMonthDto> GetMyMonthAsync(int userId, int year, int month, CancellationToken cancellationToken = default)
    {
        var firstDay = new DateTime(year, month, 1);
        var nextMonth = firstDay.AddMonths(1);

        var schedule = await _calendar.GetScheduleByUserIdAsync(userId, cancellationToken);
        var holidays = (await _calendar.GetHolidaysAsync(firstDay, nextMonth, cancellationToken))
            .ToDictionary(h => h.Date.Date, h => h.Name);

        // No stored schedule = fully remote; weekends and holidays still win.
        var officeDaysMask = schedule?.OfficeDaysMask ?? 0;

        var result = new CalendarMonthDto { Year = year, Month = month };
        for (var day = firstDay; day < nextMonth; day = day.AddDays(1))
        {
            result.Days.Add(new CalendarDayDto
            {
                Date = day.ToString("yyyy-MM-dd"),
                Kind = Classify(day, officeDaysMask, holidays, out var holidayName).ToString(),
                HolidayName = holidayName
            });
        }

        return result;
    }

    private static WorkDayKind Classify(
        DateTime day,
        int officeDaysMask,
        IReadOnlyDictionary<DateTime, string> holidays,
        out string? holidayName)
    {
        holidayName = null;

        if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return WorkDayKind.Weekend;
        }

        if (holidays.TryGetValue(day.Date, out var name))
        {
            holidayName = name;
            return WorkDayKind.Holiday;
        }

        // OfficeDaysMask bit 0 = Monday ... 6 = Sunday; DayOfWeek has Sunday = 0.
        var mondayBasedIndex = ((int)day.DayOfWeek + 6) % 7;
        return (officeDaysMask & (1 << mondayBasedIndex)) != 0
            ? WorkDayKind.Office
            : WorkDayKind.Remote;
    }
}
