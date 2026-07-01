namespace Meridian.Api.Authorization;

// Authorization policy names. Resource/role aware — not flat role gates.
//   CanRead         : any authenticated user may read.
//   HrWrite         : HR only — writes on templates/schedule (endpoints land in later slices).
//   BoardOwnerWrite : owner only — a new hire may write on their own board (later slices).
// Managers are read-only everywhere: they satisfy CanRead but no write policy.
public static class Policies
{
    public const string CanRead = "CanRead";
    public const string HrWrite = "HrWrite";
    public const string BoardOwnerWrite = "BoardOwnerWrite";
}
