using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping + seed for template cards. One realistic default set across
// the categories: safety material, resource links, and direct contacts for
// team/manager/HR/IT. All values constant — deterministic, idempotent seed.
public class TemplateCardConfiguration : IEntityTypeConfiguration<TemplateCard>
{
    public void Configure(EntityTypeBuilder<TemplateCard> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.Type).IsRequired().HasConversion<int>();
        builder.Property(c => c.Url).HasMaxLength(2048);
        builder.Property(c => c.Order).IsRequired();

        builder.HasIndex(c => new { c.TemplateId, c.Order });

        builder.HasData(
            new TemplateCard
            {
                Id = 1,
                TemplateId = 1,
                Title = "Workplace safety basics",
                Description = "Required reading before your first day on site: evacuation routes, incident reporting and protective equipment.",
                Type = CardType.Safety,
                Url = "https://intranet.meridian.local/safety/basics",
                Order = 1
            },
            new TemplateCard
            {
                Id = 2,
                TemplateId = 1,
                Title = "Employee handbook",
                Description = "Company policies, benefits and day-to-day practicalities.",
                Type = CardType.Resource,
                Url = "https://intranet.meridian.local/handbook",
                Order = 2
            },
            new TemplateCard
            {
                Id = 3,
                TemplateId = 1,
                Title = "Development environment setup",
                Description = "Step-by-step guide to get your workstation and accounts ready.",
                Type = CardType.Resource,
                Url = "https://intranet.meridian.local/it/dev-setup",
                Order = 3
            },
            new TemplateCard
            {
                Id = 4,
                TemplateId = 1,
                Title = "Meet your team",
                Description = "Your team's channel — say hello and find your onboarding buddy.",
                Type = CardType.Contact,
                Url = "https://chat.meridian.local/channels/team",
                Order = 4
            },
            new TemplateCard
            {
                Id = 5,
                TemplateId = 1,
                Title = "Your manager",
                Description = "Direct line to your manager for questions and 1:1 scheduling.",
                Type = CardType.Contact,
                Url = "mailto:manager@meridian.local",
                Order = 5
            },
            new TemplateCard
            {
                Id = 6,
                TemplateId = 1,
                Title = "HR contact",
                Description = "Contracts, payroll and anything people-related.",
                Type = CardType.Contact,
                Url = "mailto:hr@meridian.local",
                Order = 6
            },
            new TemplateCard
            {
                Id = 7,
                TemplateId = 1,
                Title = "IT helpdesk",
                Description = "Hardware, accounts and access issues.",
                Type = CardType.Contact,
                Url = "mailto:it@meridian.local",
                Order = 7
            });
    }
}
