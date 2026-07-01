namespace Meridian.Bll.Dtos;

// Template card projection exposed across the API boundary (never the entity).
public class TemplateCardDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? Url { get; set; }

    public int Order { get; set; }
}
