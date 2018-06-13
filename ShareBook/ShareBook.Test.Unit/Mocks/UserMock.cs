using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ShareBook.Test.Unit.Mocks
{
    public class UserMock
    {
        public ClaimsPrincipal GetClaimsUser()
        {
            ClaimsIdentity identity = new ClaimsIdentity(
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, new Guid().ToString())
                    }
                );

            identity.AddClaim(new Claim(ClaimTypes.Name, Guid.NewGuid().ToString()));

            ClaimsPrincipal principal = new ClaimsPrincipal(identity);

            return principal;
        }
    }
}
