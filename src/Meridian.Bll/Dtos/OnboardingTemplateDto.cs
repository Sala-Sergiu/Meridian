namespace Meridian.Bll.Dtos;

// Onboarding template projection exposed across the API boundary.
public class OnboardingTemplateDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<TemplateCardDto> Cards { get; set; } = new();
}
