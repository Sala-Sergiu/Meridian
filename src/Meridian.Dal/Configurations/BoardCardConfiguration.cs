using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping for cards on a hire's board. Content columns mirror the
// template card they were cloned from; Status carries the hire's progress.
public class BoardCardConfiguration : IEntityTypeConfiguration<BoardCard>
{
    public void Configure(EntityTypeBuilder<BoardCard> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(1000);
        builder.Property(c => c.Type).IsRequired().HasConversion<int>();
        builder.Property(c => c.Url).HasMaxLength(2048);
        builder.Property(c => c.Order).IsRequired();
        builder.Property(c => c.Status).IsRequired().HasConversion<int>();

        builder.HasIndex(c => new { c.BoardId, c.Order });
    }
}
