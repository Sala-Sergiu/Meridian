using Mapster;
using Meridian.Bll.Dtos;
using Meridian.Bll.Security;
using Meridian.Domain.Repositories;

namespace Meridian.Bll.Services;

// Login flow: look up user, verify password, issue a token.
public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResultDto?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!_passwordHasher.Verify(password, user.PasswordHash))
        {
            return null;
        }

        var token = _tokenService.CreateToken(user);

        return new LoginResultDto
        {
            Token = token,
            User = user.Adapt<UserDto>()
        };
    }
}
