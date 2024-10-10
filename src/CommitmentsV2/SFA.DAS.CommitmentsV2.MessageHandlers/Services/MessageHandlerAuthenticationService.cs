using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.Services;

/// <summary>
/// IAuthenticationService service is required by some commandhandlers. The Concrete implementation of this service
/// uses IHttpContext which is not available for SFA.DAS.CommitmentsV2.MessageHandler which does not expose request
/// endpoints.
/// </summary>
[ExcludeFromCodeCoverage]
public class MessageHandlerAuthenticationService : IAuthenticationService
{
    private const string ExceptionMessage = "Can not authenticate events";

    public AuthenticationServiceType AuthenticationServiceType => AuthenticationServiceType.MessageHandler;

    public IEnumerable<string> GetAllUserRoles()
    {
        throw new NotImplementedException(ExceptionMessage);
    }

    public Party GetUserParty()
    {
        throw new NotImplementedException(ExceptionMessage);
    }

    public bool IsUserInRole(string role)
    {
        throw new NotImplementedException(ExceptionMessage);
    }
}