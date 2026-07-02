namespace Meridian.Bll.Dtos;

// Optional query parameters for reading a board's cards. Enum-ish values
// arrive as strings from the query string and are validated at the API edge
// (BoardCardsQueryValidator) before the pipeline is composed.
public class BoardCardsQueryDto
{
    // Optional CardStatus name (ToDo/InProgress/Done), case-insensitive.
    public string? Status { get; set; }

    // Optional CardType name (Resource/Safety/Contact), case-insensitive.
    public string? Type { get; set; }

    // Sort direction over the card Order: "asc" (default) or "desc".
    public string? Sort { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
