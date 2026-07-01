using Meridian.Bll.Dtos;

namespace Meridian.Bll.Services;

// Authentication business logic. All login logic lives here, not in controllers.
public interface IAuthService
{
    // Returns the login result on success, or null when the email is unknown
    // or the password does not match.
    Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
