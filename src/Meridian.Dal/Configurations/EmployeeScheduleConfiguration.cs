using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping + seed for EmployeeSchedule. Every seeded user works the
// same 3+2 hybrid default (Mon–Wed office, Thu–Fri remote, mask 0b111 = 7);
// per-person changes are an HR concern later.
public class EmployeeScheduleConfiguration : IEntityTypeConfiguration<EmployeeSchedule>
{
    public void Configure(EntityTypeBuilder<EmployeeSchedule> builder)
    {
        builder.HasKey(s => s.Id);
        builder.HasIndex(s => s.UserId).IsUnique();
        builder.Property(s => s.OfficeDaysMask).IsRequired();

        // FK to User without a navigation — schedules hang off users, but the
        // domain never traverses from one to the other.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(
            new EmployeeSchedule { Id = 1, UserId = 1, OfficeDaysMask = 7 },
            new EmployeeSchedule { Id = 2, UserId = 2, OfficeDaysMask = 7 },
            new EmployeeSchedule { Id = 3, UserId = 3, OfficeDaysMask = 7 });
    }
}
