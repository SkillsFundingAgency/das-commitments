using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.CommitmentsV2.Api.Authorization
{
    public class LocalAuthorizationHandler : AuthorizationHandler<IsDevelopmentRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            IsDevelopmentRequirement requirement)
        {
            if (requirement.IsDevelopment)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}