﻿using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace SFA.DAS.CommitmentsV2.Api.Authentication;

public class AzureAdScopeClaimTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {

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

    private static class Constants
    {
        public const string ScopeClaimType = "http://schemas.microsoft.com/identity/claims/scope";
    }
}