namespace Meridian.Bll.Dtos;

// Board card projection exposed across the API boundary (never the entity).
public class BoardCardDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Url { get; set; }

    public int Order { get; set; }

    public string Status { get; set; } = string.Empty;
}
