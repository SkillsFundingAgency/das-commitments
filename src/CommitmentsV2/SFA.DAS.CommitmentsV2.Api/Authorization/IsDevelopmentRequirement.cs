using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.CommitmentsV2.Api.Authorization
{
    public class IsDevelopmentRequirement : IAuthorizationRequirement
    {
        public bool IsDevelopment { get; }
        public IsDevelopmentRequirement(bool isDevelopment)
        {
            IsDevelopment = isDevelopment;
        }
    }
}