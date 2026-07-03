namespace Meridian.Bll.Dtos;

// HR publishes a new onboarding article: it is appended to the template (so
// future hires get it via cloning) AND pushed to every existing board.
public class PublishCardRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // CardType name — Safety (required reading) or Resource (task). Contacts
    // are reference data managed with the template, not broadcast.
    public string Type { get; set; } = string.Empty;

    public string? Url { get; set; }
}

// Outcome of a publish: the card as it landed on the template, plus how many
// existing boards received a copy.
public class PublishCardResultDto
{
    public TemplateCardDto Card { get; set; } = new();

    public int BoardsUpdated { get; set; }
}
