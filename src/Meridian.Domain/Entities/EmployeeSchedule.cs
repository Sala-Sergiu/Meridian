namespace Meridian.Domain.Entities;

// Weekly hybrid-work pattern for one employee (Meridian works 3 days office,
// 2 days remote — which days is per person).
public class EmployeeSchedule
{
    public int Id { get; set; }

    public int UserId { get; set; }

    // Bit i set = day i of the week is an office day, with 0 = Monday through
    // 6 = Sunday. E.g. Monday+Tuesday+Wednesday = 0b0000111 = 7.
    public int OfficeDaysMask { get; set; }
}
