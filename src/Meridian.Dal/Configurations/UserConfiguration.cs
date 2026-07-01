using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Dal.Configurations;

// EF Core mapping + seed for User.
//
// Seed passwords are hashed with BCrypt, whose salt is random — calling the
// hasher inside HasData would produce a different hash every run and make EF
// detect a model change on each migration. The hashes below were pre-computed
// ONCE (work factor 11) and hardcoded so the seed is deterministic/idempotent.
//
// Dev/demo login credentials (Meridian login only — NOT any external credential):
//   newhire@meridian.local  / NewHire#123
//   hr@meridian.local       / HrAdmin#123
//   manager@meridian.local  / Manager#123
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(256);
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.DisplayName).IsRequired().HasMaxLength(128);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(256);
        builder.Property(u => u.Role).IsRequired().HasConversion<int>();

        builder.HasData(
            new User
            {
                Id = 1,
                Email = "newhire@meridian.local",
                DisplayName = "Nadia NewHire",
                PasswordHash = "$2a$11$KXtREvvcnStxCV6zgiZM6.TJpeMcEwQ1vn4jSljs24Z8MTrCHazrC",
                Role = Role.NewHire
            },
            new User
            {
                Id = 2,
                Email = "hr@meridian.local",
                DisplayName = "Hannah HR",
                PasswordHash = "$2a$11$Uu62dUNUzOfsiD9yy38yfehjpeoYwRwYqEEqdZ.8dq7BsFn1TvXXi",
                Role = Role.HR
            },
            new User
            {
                Id = 3,
                Email = "manager@meridian.local",
                DisplayName = "Marcus Manager",
                PasswordHash = "$2a$11$gIxvyg/V6Qbjt5HrOmhiYOWE/0kbmsYsgRosAa4jdWZTSszDh8lZm",
                Role = Role.Manager
            });
    }
}
