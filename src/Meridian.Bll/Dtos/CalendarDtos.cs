namespace Meridian.Bll.Dtos;

// One day in an employee's month view.
public class CalendarDayDto
{
    // ISO date (yyyy-MM-dd) — unambiguous for the frontend.
    public string Date { get; set; } = string.Empty;

    // WorkDayKind name: Office, Remote, Holiday or Weekend.
    public string Kind { get; set; } = string.Empty;

    public string? HolidayName { get; set; }
}

// A full month of the employee's work calendar.
public class CalendarMonthDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public List<CalendarDayDto> Days { get; set; } = new();
}

// Query for the my-calendar endpoint; missing values default to the current
// month server-side.
public class CalendarQueryDto
{
    public int? Year { get; set; }

    public int? Month { get; set; }
}
