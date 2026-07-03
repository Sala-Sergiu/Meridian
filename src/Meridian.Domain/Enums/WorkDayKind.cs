namespace Meridian.Domain.Enums;

// What a given calendar day is for an employee. Weekend and Holiday win over
// the office/remote pattern.
public enum WorkDayKind
{
    Office = 0,
    Remote = 1,
    Holiday = 2,
    Weekend = 3
}
