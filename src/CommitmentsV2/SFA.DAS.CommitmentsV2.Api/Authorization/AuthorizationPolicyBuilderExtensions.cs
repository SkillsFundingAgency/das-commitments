using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.CommitmentsV2.Api.Authorization;

public static class AuthorizationPolicyBuilderExtensions
{
    public static AuthorizationPolicyBuilder AllowAnonymousUser(this AuthorizationPolicyBuilder builder)
    {
        builder.Requirements.Add(new NoneRequirement());
        return builder;
    }
}