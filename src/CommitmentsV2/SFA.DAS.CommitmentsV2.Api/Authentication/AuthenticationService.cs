using System;
using Microsoft.AspNetCore.Authentication;
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

        public Party GetUserParty()
        {
            var isEmployer = IsUserInRole(Role.Employer);
            var isProvider = IsUserInRole(Role.Provider);

            // The client app should be one _or_ the other (not both, not neither).
            if (isEmployer ^ isProvider)
            {
                return isEmployer ? Party.Employer : Party.Provider;
            }

            //This method will need to be modified to deal with Transfer Sender in future. This is because the Transfer Sender will have the Employer role.
            //In this case, this method will need to be provided with some sort of flag indicating how to treat users in the Employer role.
            //Specific endpoints will be provided for the Transfer Sender functionality, so this will be possible.
            //One possibility is to add an alternative implementation for this for transfer senders??

            throw new ArgumentException($"Unable to map User Role (IsEmployer:{isEmployer}, IsProvider:{isProvider}) to Party");
        }
    }
}
