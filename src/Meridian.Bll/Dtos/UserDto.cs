namespace Meridian.Bll.Dtos;

// User projection exposed across the API boundary (never the entity).
public class UserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
}
