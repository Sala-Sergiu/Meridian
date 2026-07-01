using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Persistence;

// EF Core unit of work. No separate IUnitOfWork — the DbContext is the unit of work.
public class MeridianDbContext : DbContext
{
    public MeridianDbContext(DbContextOptions<MeridianDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<OnboardingTemplate> OnboardingTemplates => Set<OnboardingTemplate>();

    public DbSet<TemplateCard> TemplateCards => Set<TemplateCard>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeridianDbContext).Assembly);
    }
}
