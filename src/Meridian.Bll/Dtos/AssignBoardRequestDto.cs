namespace Meridian.Bll.Dtos;

// HR request to assign onboarding: clone the template into a board for a hire.
public class AssignBoardRequestDto
{
    public int TemplateId { get; set; }

    public int HireUserId { get; set; }
}
