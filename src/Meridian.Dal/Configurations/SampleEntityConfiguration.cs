using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core fluent configuration + HasData seed for SampleEntity.
public class SampleEntityConfiguration : IEntityTypeConfiguration<SampleEntity>
{
    public void Configure(EntityTypeBuilder<SampleEntity> builder)
    {
        // TODO: map keys, properties, relationships and seed data per spec.
    }
}
