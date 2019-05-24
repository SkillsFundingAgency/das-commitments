using Microsoft.AspNetCore.Http;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;
using IAuthenticationService = SFA.DAS.CommitmentsV2.Authentication.IAuthenticationService;

namespace SFA.DAS.CommitmentsV2.Api.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool IsUserInRole(string role)
        {
            return _httpContextAccessor.HttpContext.User.IsInRole(role);
        }

        public Originator GetUserRole()
        {
            var isEmployer = IsUserInRole(Role.Employer);
            var isProvider = IsUserInRole(Role.Provider);

            // The client app should be one _or_ the other (not both, not neither). If this is not the case then something is wrong.
            if (isEmployer ^ isProvider)
            {
                return isEmployer ? Originator.Employer : Originator.Provider;
            }

            return Originator.Unknown;
        }
    }
}