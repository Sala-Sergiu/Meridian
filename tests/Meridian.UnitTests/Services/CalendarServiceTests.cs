using Meridian.Bll.Services;
using Meridian.Domain.Entities;
using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

public class CalendarServiceTests
{
    private readonly IWorkCalendarRepository _repository = Substitute.For<IWorkCalendarRepository>();
    private readonly CalendarService _service;

    public CalendarServiceTests()
    {
        _service = new CalendarService(_repository);
        _repository.GetHolidaysAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<PublicHoliday>());
    }

    private void ScheduleIs(EmployeeSchedule? schedule) =>
        _repository.GetScheduleByUserIdAsync(1, Arg.Any<CancellationToken>()).Returns(schedule);

    [Fact]
    public async Task Classifies_office_and_remote_days_from_the_mask()
    {
        // Mon+Tue+Wed office (bits 0..2).
        ScheduleIs(new EmployeeSchedule { Id = 1, UserId = 1, OfficeDaysMask = 7 });

        // June 2026: the 1st is a Monday.
        var month = await _service.GetMyMonthAsync(1, 2026, 6);

        Assert.Equal(30, month.Days.Count);
        Assert.Equal("Office", month.Days[0].Kind);  // Mon 1st
        Assert.Equal("Office", month.Days[2].Kind);  // Wed 3rd
        Assert.Equal("Remote", month.Days[3].Kind);  // Thu 4th
        Assert.Equal("Remote", month.Days[4].Kind);  // Fri 5th
    }

    [Fact]
    public async Task Weekends_win_over_the_office_mask()
    {
        // All seven bits set: even weekend bits must not override Weekend.
        ScheduleIs(new EmployeeSchedule { Id = 1, UserId = 1, OfficeDaysMask = 0b1111111 });

        var month = await _service.GetMyMonthAsync(1, 2026, 6);

        Assert.Equal("Weekend", month.Days[5].Kind); // Sat 6th
        Assert.Equal("Weekend", month.Days[6].Kind); // Sun 7th
    }

    [Fact]
    public async Task Holidays_win_over_office_days_and_carry_their_name()
    {
        ScheduleIs(new EmployeeSchedule { Id = 1, UserId = 1, OfficeDaysMask = 7 });
        _repository.GetHolidaysAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new List<PublicHoliday>
            {
                // Mon 1 June 2026 — an office day by mask.
                new() { Id = 1, Date = new DateTime(2026, 6, 1), Name = "Pentecost Monday & Children's Day" }
            });

        var month = await _service.GetMyMonthAsync(1, 2026, 6);

        Assert.Equal("Holiday", month.Days[0].Kind);
        Assert.Equal("Pentecost Monday & Children's Day", month.Days[0].HolidayName);
        Assert.Equal("Office", month.Days[1].Kind); // Tue 2nd unaffected
    }

    [Fact]
    public async Task Missing_schedule_defaults_to_remote_weekdays()
    {
        ScheduleIs(null);

        var month = await _service.GetMyMonthAsync(1, 2026, 6);

        Assert.Equal("Remote", month.Days[0].Kind);   // Mon 1st
        Assert.Equal("Weekend", month.Days[5].Kind);  // Sat 6th
    }

    [Fact]
    public async Task Days_are_emitted_as_iso_dates_for_the_whole_month()
    {
        ScheduleIs(null);

        var month = await _service.GetMyMonthAsync(1, 2026, 2);

        Assert.Equal(28, month.Days.Count);
        Assert.Equal("2026-02-01", month.Days[0].Date);
        Assert.Equal("2026-02-28", month.Days[^1].Date);
    }
}
