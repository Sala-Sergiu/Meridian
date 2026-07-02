namespace Meridian.Bll.Dtos;

// Per-hire onboarding board projection exposed across the API boundary.
public class OnboardingBoardDto
{
    public int Id { get; set; }

    public int HireUserId { get; set; }

    public DateTime AssignedAt { get; set; }

    public List<BoardCardDto> Cards { get; set; } = new();
}
