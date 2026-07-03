namespace Meridian.Domain.Entities;

// A legal public holiday — company-wide, not per employee.
public class PublicHoliday
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public string Name { get; set; } = string.Empty;
}
