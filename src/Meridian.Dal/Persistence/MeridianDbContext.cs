using Microsoft.EntityFrameworkCore;

namespace Meridian.Dal.Persistence;

// EF Core unit of work. No separate IUnitOfWork — the DbContext is the unit of work.
public class MeridianDbContext : DbContext
{
    public MeridianDbContext(DbContextOptions<MeridianDbContext> options)
        : base(options)
    {
    }

    // DbSet<T> properties to be added per spec.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: apply IEntityTypeConfiguration<T> from this assembly and HasData seed (idempotent by key).
    }
}
