namespace Meridian.Bll.Dtos;

// Outcome of an assignment. AlreadyExisted distinguishes a fresh clone from
// the idempotent repeat case so the API can answer 201 vs 200.
public class AssignBoardResultDto
{
    public OnboardingBoardDto Board { get; set; } = new();

    public bool AlreadyExisted { get; set; }
}
