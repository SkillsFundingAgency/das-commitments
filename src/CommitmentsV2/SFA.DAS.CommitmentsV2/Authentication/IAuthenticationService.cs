using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Authentication
{
    public interface IAuthenticationService
    {
        AuthenticationServiceType AuthenticationServiceType { get; }
        IEnumerable<string> GetAllUserRoles();
        bool IsUserInRole(string role);
        Party GetUserParty();
    }

    /// <summary>
    /// This enum identifies the type of the AuthenticationService implementation.
    /// </summary>
    /// <remarks>
    /// In some cases, the implementation of IAuthenticationService uses HttpContext, which is not available in the MessageHandler project.
    /// At this point MessageHandler requests are trusted, so information is derived from the message itself, and IAuthenticationService
    /// will throw exceptions if used.
    /// </remarks>
    public enum AuthenticationServiceType
    {
        HttpContext,
        MessageHandler
    }
}