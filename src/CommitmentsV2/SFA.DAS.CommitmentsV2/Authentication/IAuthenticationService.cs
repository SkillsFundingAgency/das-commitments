using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Authentication
{
    public interface IAuthenticationService
    {
        IEnumerable<string> GetAllUserRoles();
        bool IsUserInRole(string role);
        Party GetUserParty();
    }
}