using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping + seed for the HR-owned onboarding template.
// Seed values are constants (fixed ids/strings) so HasData stays deterministic
// and idempotent — re-running migrations never duplicates rows.
public class OnboardingTemplateConfiguration : IEntityTypeConfiguration<OnboardingTemplate>
{
    public void Configure(EntityTypeBuilder<OnboardingTemplate> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name).IsRequired().HasMaxLength(200);

        builder.HasMany(t => t.Cards)
            .WithOne()
            .HasForeignKey(c => c.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(new OnboardingTemplate
        {
            Id = 1,
            Name = "Default Onboarding"
        });
    }
}
