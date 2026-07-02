namespace Meridian.Api.Authorization;

// Authorization policy names. Resource/role aware — not flat role gates.
//   CanRead         : any authenticated user may read.
//   HrWrite         : HR only — writes on templates/schedule and board assignment.
//   HrOrManagerRead : HR/Manager only — read any hire's board (progress overview).
//                     A NewHire never satisfies this; they read their own board
//                     via /me, where the id comes from their JWT.
//   BoardOwnerWrite : owner only — a new hire may write on their own board (later slices).
// Managers are read-only everywhere: they satisfy the read policies but no write policy.
public static class Policies
{
    public const string CanRead = "CanRead";
    public const string HrWrite = "HrWrite";
    public const string HrOrManagerRead = "HrOrManagerRead";
    public const string BoardOwnerWrite = "BoardOwnerWrite";
}
