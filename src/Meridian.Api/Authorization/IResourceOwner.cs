namespace Meridian.Api.Authorization;

// Implemented by resources that belong to a specific user (e.g. a new hire's
// onboarding board). Used by BoardOwnerWrite for resource-based authorization.
public interface IResourceOwner
{
    int OwnerUserId { get; }
}
