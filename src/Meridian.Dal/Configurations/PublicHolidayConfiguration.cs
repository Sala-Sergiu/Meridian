using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping + seed for PublicHoliday: the Romanian legal holidays for
// 2026, as fixed dates (constants — HasData must be deterministic). Future
// years get seeded the same way when the time comes.
public class PublicHolidayConfiguration : IEntityTypeConfiguration<PublicHoliday>
{
    public void Configure(EntityTypeBuilder<PublicHoliday> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(128);
        builder.HasIndex(h => h.Date).IsUnique();

        builder.HasData(
            new PublicHoliday { Id = 1, Date = new DateTime(2026, 1, 1), Name = "New Year's Day" },
            new PublicHoliday { Id = 2, Date = new DateTime(2026, 1, 2), Name = "Day after New Year" },
            new PublicHoliday { Id = 3, Date = new DateTime(2026, 1, 6), Name = "Epiphany" },
            new PublicHoliday { Id = 4, Date = new DateTime(2026, 1, 7), Name = "St John the Baptist" },
            new PublicHoliday { Id = 5, Date = new DateTime(2026, 1, 24), Name = "Union Day" },
            new PublicHoliday { Id = 6, Date = new DateTime(2026, 4, 10), Name = "Orthodox Good Friday" },
            new PublicHoliday { Id = 7, Date = new DateTime(2026, 4, 12), Name = "Orthodox Easter" },
            new PublicHoliday { Id = 8, Date = new DateTime(2026, 4, 13), Name = "Orthodox Easter Monday" },
            new PublicHoliday { Id = 9, Date = new DateTime(2026, 5, 1), Name = "Labour Day" },
            new PublicHoliday { Id = 10, Date = new DateTime(2026, 5, 31), Name = "Orthodox Pentecost" },
            new PublicHoliday { Id = 11, Date = new DateTime(2026, 6, 1), Name = "Pentecost Monday & Children's Day" },
            new PublicHoliday { Id = 12, Date = new DateTime(2026, 8, 15), Name = "Assumption of Mary" },
            new PublicHoliday { Id = 13, Date = new DateTime(2026, 11, 30), Name = "St Andrew's Day" },
            new PublicHoliday { Id = 14, Date = new DateTime(2026, 12, 1), Name = "National Day" },
            new PublicHoliday { Id = 15, Date = new DateTime(2026, 12, 25), Name = "Christmas Day" },
            new PublicHoliday { Id = 16, Date = new DateTime(2026, 12, 26), Name = "Second Day of Christmas" });
    }
}
