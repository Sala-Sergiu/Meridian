using Meridian.Domain.Entities;

namespace Meridian.Bll.Security;

// Issues a signed JWT for an authenticated user.
// The concrete implementation lives in the Api layer (JWT signing/config),
// keeping the Bll free of any web/JWT infrastructure dependency.
public interface ITokenService
{
    string CreateToken(User user);
}
