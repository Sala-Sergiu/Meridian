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
                // Relative urls are in-app resource pages rendered by the frontend.
                Url = "/resources/safety-basics",
                Order = 1
            },
            new TemplateCard
            {
                Id = 2,
                TemplateId = 1,
                Title = "Employee handbook",
                Description = "Company policies, benefits and day-to-day practicalities.",
                Type = CardType.Resource,
                Url = "/resources/employee-handbook",
                Order = 2
            },
            new TemplateCard
            {
                Id = 3,
                TemplateId = 1,
                Title = "Development environment setup",
                Description = "Step-by-step guide to get your workstation and accounts ready.",
                Type = CardType.Resource,
                Url = "/resources/dev-setup",
                Order = 3
            },
            new TemplateCard
            {
                Id = 4,
                TemplateId = 1,
                Title = "Meet your team",
                Description = "Your team's channel — say hello and find your onboarding buddy.",
                Type = CardType.Contact,
                Url = "/resources/meet-your-team",
                Order = 4
            },
            new TemplateCard
            {
                Id = 5,
                TemplateId = 1,
                Title = "Your manager",
                Description = "Direct line to your manager for questions and 1:1 scheduling.",
                Type = CardType.Contact,
                Url = "/resources/your-manager",
                Order = 5
            },
            new TemplateCard
            {
                Id = 6,
                TemplateId = 1,
                Title = "HR contact",
                Description = "Contracts, payroll and anything people-related.",
                Type = CardType.Contact,
                Url = "/resources/hr-contact",
                Order = 6
            },
            new TemplateCard
            {
                Id = 7,
                TemplateId = 1,
                Title = "IT helpdesk",
                Description = "Hardware, accounts and access issues.",
                Type = CardType.Contact,
                Url = "/resources/it-helpdesk",
                Order = 7
            },
            new TemplateCard
            {
                Id = 8,
                TemplateId = 1,
                Title = "Data & client confidentiality",
                Description = "Company and client information stays inside the company — required reading.",
                Type = CardType.Safety,
                Url = "/resources/data-confidentiality",
                Order = 8
            });
    }
}
