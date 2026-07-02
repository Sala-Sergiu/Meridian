namespace Meridian.Bll.Dtos;

// Request to move a board card to a new progress column.
public class MoveCardRequestDto
{
    // Target CardStatus name (ToDo/InProgress/Done), case-insensitive.
    public string Status { get; set; } = string.Empty;
}
