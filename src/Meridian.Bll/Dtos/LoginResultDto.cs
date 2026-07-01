namespace Meridian.Bll.Dtos;

// Result of a successful login: the signed JWT plus basic user info.
public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;

    public UserDto User { get; set; } = new();
}
