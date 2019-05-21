using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Authentication
{
    public interface IAuthenticationService
    {
        bool IsUserInRole(string role);
        Originator GetUserRole();
    }
}