using System.Security.Claims;
using System.Security.Principal;

namespace SFA.DAS.ProviderCommitments.Extensions
{
    public static class IdentityExtensions
    {
        public static string GetClaim(this IIdentity identity, string claim)
        {
            var claimsPrincipal = new ClaimsPrincipal(identity);

            return claimsPrincipal.FindFirst(c => c.Type == claim).Value;
        }
    }
}
