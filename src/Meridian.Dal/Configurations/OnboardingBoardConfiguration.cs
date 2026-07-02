using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping for per-hire onboarding boards. No HasData here on purpose:
// boards are created by assignment (cloning the template) — cloning is the
// single source of truth, keeping the demo flow real.
public class OnboardingBoardConfiguration : IEntityTypeConfiguration<OnboardingBoard>
{
    public void Configure(EntityTypeBuilder<OnboardingBoard> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.HireUserId).IsRequired();
        builder.Property(b => b.AssignedAt).IsRequired();

        // One board per hire — the DB backs up the guard in the assign service.
        builder.HasIndex(b => b.HireUserId).IsUnique();

        // FK to the owning hire without a navigation property (Domain stays lean).
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.HireUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Cards)
            .WithOne()
            .HasForeignKey(c => c.BoardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
