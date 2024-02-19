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

        public IEnumerable<string> GetAllUserRoles()
        {
            return _httpContextAccessor.HttpContext.User.Identities.SelectMany(i => i.FindAll(i.RoleClaimType).Select(c => c.Value));
        }

        public bool IsUserInRole(string role)
        {
            return _httpContextAccessor.HttpContext.User.IsInRole(role);
        }

        public Party GetUserParty()
        {
            var isEmployer = IsUserInRole(Role.Employer);
            var isProvider = IsUserInRole(Role.Provider);

            // The client app should be one _or_ the other (not both, not neither).
            if (isEmployer ^ isProvider)
            {
                return isEmployer ? Party.Employer : Party.Provider;
            }

            //This method may need revisiting in future, as it does not support TransferSenders, who are in the Employer role. Specific endpoints will be
            //made available for TransferSender functionality, so perhaps it doesn't matter - in this case, we would just need to assert that the user is
            //in the Employer role and thereby infer that they must be the TransferSender in that context. Alternatively, could another implementation of this
            //be created for use within the TransferSender functionality? This would assert that the user is in the Employer role, and return TransferSender
            //as the Party, or otherwise throw an exception.

            throw new ArgumentException($"Unable to map User Role (IsEmployer:{isEmployer}, IsProvider:{isProvider}) to Party");
        }
    }
}
