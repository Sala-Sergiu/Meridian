namespace Meridian.Api.Configuration;

// Strongly-typed JWT settings bound from the "Jwt" configuration section.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}
