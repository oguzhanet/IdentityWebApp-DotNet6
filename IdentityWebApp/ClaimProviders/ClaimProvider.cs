using IdentityWebApp.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace IdentityWebApp.ClaimProviders
{
    public class ClaimProvider : IClaimsTransformation
    {
        private readonly UserManager<AppUser> _userManager;

        public ClaimProvider(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal != null && principal.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = principal.Identity as ClaimsIdentity;

                AppUser user = await _userManager.FindByNameAsync(identity.Name);

                if (user != null)
                {
                    if (user.City != null)
                    {
                        if (!principal.HasClaim(x=>x.Type == "city"))
                        {
                            Claim cityClaim = new("city", user.City, ClaimValueTypes.String, "Internal");
                            identity.AddClaim(cityClaim);
                        }
                    }
                }
            }

            return principal;
        }
    }
}
