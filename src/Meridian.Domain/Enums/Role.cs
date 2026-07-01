namespace Meridian.Domain.Enums;

// Application role. Drives authorization policies (HR write, manager read-only, new hire owns own board).
public enum Role
{
    NewHire = 0,
    HR = 1,
    Manager = 2
}
