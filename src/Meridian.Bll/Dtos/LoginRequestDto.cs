namespace Meridian.Bll.Dtos;

// Login request payload. No self-registration exists — login only.
public class LoginRequestDto
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
