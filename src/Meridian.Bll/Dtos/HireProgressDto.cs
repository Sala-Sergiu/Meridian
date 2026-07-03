namespace Meridian.Bll.Dtos;

// One row in the HR/Manager tracking view: a new hire and how far along
// their onboarding is. Contacts are reference info and never counted.
public class HireProgressDto
{
    public int HireUserId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool HasBoard { get; set; }

    // Resource cards (the Kanban tasks).
    public int TasksDone { get; set; }

    public int TasksTotal { get; set; }

    // Safety cards (required reading).
    public int ReadDone { get; set; }

    public int ReadTotal { get; set; }
}
