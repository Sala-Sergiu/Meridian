using Meridian.Domain.Common;
using Meridian.Domain.Enums;

namespace Meridian.Domain.Entities;

// A card on a hire's board. Content fields (title, description, type, url,
// order) are copied from the template card at clone time; the status carries
// the hire's own progress.
public class BoardCard : BaseEntity
{
    public int BoardId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public CardType Type { get; set; }

    // Set for resource/contact links; null for cards without a link target.
    public string? Url { get; set; }

    public int Order { get; set; }

    public CardStatus Status { get; set; } = CardStatus.ToDo;
}
