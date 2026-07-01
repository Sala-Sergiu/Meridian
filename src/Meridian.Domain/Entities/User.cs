using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

// A provisioned (seeded) application user. No self-registration.
// PasswordHash holds ONLY a BCrypt hash of the Meridian login password —
// never plaintext, and never any external work/physical-access credential.
public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public Role Role { get; set; }
}
