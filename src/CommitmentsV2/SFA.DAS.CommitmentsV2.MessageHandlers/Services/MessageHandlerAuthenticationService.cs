using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.Services
{
    /// <summary>
    /// IAuthenticationService service is required by some commandhandlers. The Concrete implementation of this service
    /// uses IHttpContext which is not available for SFA.DAS.CommitmentsV2.MessageHandler which does not expose request
    /// endpoints.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MessageHandlerAuthenticationService : IAuthenticationService
    {

        private readonly string _exceptionMessage = "Can not authenticate events";

        public AuthenticationServiceType AuthenticationServiceType => AuthenticationServiceType.MessageHandler;

        public IEnumerable<string> GetAllUserRoles()
        {
            throw new NotImplementedException(_exceptionMessage);
        }

        public Party GetUserParty()
        {
            throw new NotImplementedException(_exceptionMessage);
        }

        public bool IsUserInRole(string role)
        {
            throw new NotImplementedException(_exceptionMessage);
        }
    }

}