using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

// HR-owned onboarding template. Defined once by HR, later cloned per new hire
// (cloning is a separate slice). Persistence-ignorant: no EF concerns here.
public class OnboardingTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<TemplateCard> Cards { get; set; } = new List<TemplateCard>();
}
