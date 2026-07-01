using Mapster;
using Meridian.Bll.Mapping;
using Meridian.Bll.Security;
using Meridian.Bll.Services;
using Meridian.Domain.Entities;
using Meridian.Domain.Enums;
using Meridian.Domain.Repositories;
using NSubstitute;

namespace Meridian.UnitTests.Services;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly AuthService _sut;

    static AuthServiceTests()
    {
        // Register the User -> UserDto mapping used by Adapt<T>().
        new MappingConfig().Register(TypeAdapterConfig.GlobalSettings);
    }

    public AuthServiceTests()
    {
        _sut = new AuthService(_users, _passwordHasher, _tokenService);
    }

    private static User SeededUser() => new()
    {
        Id = 7,
        Email = "hr@meridian.local",
        DisplayName = "Hannah HR",
        PasswordHash = "stored-hash",
        Role = Role.HR
    };

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsTokenAndUser()
    {
        var user = SeededUser();
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify("correct-password", user.PasswordHash).Returns(true);
        _tokenService.CreateToken(user).Returns("signed-jwt");

        var result = await _sut.LoginAsync(user.Email, "correct-password");

        Assert.NotNull(result);
        Assert.Equal("signed-jwt", result!.Token);
        Assert.Equal(user.Id, result.User.Id);
        Assert.Equal(user.Email, result.User.Email);
        Assert.Equal("HR", result.User.Role);
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ReturnsNull_AndDoesNotIssueToken()
    {
        var user = SeededUser();
        _users.GetByEmailAsync(user.Email, Arg.Any<CancellationToken>()).Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var result = await _sut.LoginAsync(user.Email, "wrong-password");

        Assert.Null(result);
        _tokenService.DidNotReceive().CreateToken(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_WithUnknownEmail_ReturnsNull_AndDoesNotVerifyOrIssueToken()
    {
        _users.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns((User?)null);

        var result = await _sut.LoginAsync("nobody@meridian.local", "whatever");

        Assert.Null(result);
        _passwordHasher.DidNotReceive().Verify(Arg.Any<string>(), Arg.Any<string>());
        _tokenService.DidNotReceive().CreateToken(Arg.Any<User>());
    }
}
