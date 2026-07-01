namespace Meridian.Bll.Security;

// Verifies a plaintext password against a stored hash.
// Only verification is needed here — users are provisioned (seeded), not registered.
public interface IPasswordHasher
{
    bool Verify(string password, string passwordHash);
}
