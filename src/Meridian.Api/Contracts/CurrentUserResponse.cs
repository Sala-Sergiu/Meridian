namespace Meridian.Api.Contracts;

// Shape returned by GET /api/users/me, resolved from the caller's JWT claims.
public record CurrentUserResponse(int Id, string Email, string DisplayName, string Role);
