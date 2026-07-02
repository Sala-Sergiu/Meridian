using Meridian.Domain.Common;

namespace Meridian.Domain.Entities;

// A hire's own onboarding board, cloned from the HR template at assignment
// time. After cloning the board is independent: later template edits do not
// touch boards already assigned. Persistence-ignorant: no EF concerns here.
public class OnboardingBoard : BaseEntity
{
    // The new hire who owns this board (User.Id).
    public int HireUserId { get; set; }

    public DateTime AssignedAt { get; set; }

    public ICollection<BoardCard> Cards { get; set; } = new List<BoardCard>();
}
