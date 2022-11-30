using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.CommitmentsV2.Api.Authentication
{
    public class AzureAdScopeClaimTransformation : IClaimsTransformation
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AzureAdScopeClaimTransformation(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            AddProviderOrEmployerClaim(principal);
            var scopeClaims = principal.FindAll(Constants.ScopeClaimType).ToList();
            if (scopeClaims.Count != 1 || !scopeClaims[0].Value.Contains(' '))
            {
                // Caller has no scopes or has multiple scopes (already split)
                // or they have only one scope
                return Task.FromResult(principal);
            }

            Claim claim = scopeClaims[0];
            string[] scopes = claim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<Claim> claims = scopes.Select(s => new Claim(Constants.ScopeClaimType, s));

            return Task.FromResult(new ClaimsPrincipal(new ClaimsIdentity(principal.Identity, claims)));
        }

        private void AddProviderOrEmployerClaim(ClaimsPrincipal principal)
        {
            var role = _httpContextAccessor.HttpContext.Request.Headers["RoleClaim"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(role) && (role.ToLower() == "provider" || role.ToLower() == "employer"))
            {
                // Remove existing claim for Role of Provider or Employer
                var claimsIdentity = principal.Identity as ClaimsIdentity;
                var claimsToRemove = claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Role && (x.Value.ToLower() == "provider" || x.Value.ToLower() == "employer"));
                foreach (var c in claimsToRemove)
                {
                    claimsIdentity.RemoveClaim(c);
                }

                // Add a new claim with provider or employer for role
                var roleEmployerOrProvider = role.ToLower() == "provider" ? "Provider" : "Employer";
                var roleClaim = new Claim(ClaimTypes.Role, roleEmployerOrProvider);
                claimsIdentity.AddClaim(roleClaim);
            }
        }

        private static class Constants
        {
            public const string ScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";
            public const string ObjectIdClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        }
    }
}
