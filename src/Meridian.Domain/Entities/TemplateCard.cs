using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

// A card on the onboarding template: an onboarding resource such as safety
// material, a resource link, or a direct contact link (team/manager/HR/IT).
public class TemplateCard : BaseEntity
{
    public int TemplateId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public CardType Type { get; set; }

    // Set for resource/contact links; null for cards without a link target.
    public string? Url { get; set; }

    public int Order { get; set; }
}
