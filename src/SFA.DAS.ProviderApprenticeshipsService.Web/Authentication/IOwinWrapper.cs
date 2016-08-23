using System.Web.Mvc;
using IdentityServer3.Core.Models;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Authentication
{
    public interface IOwinWrapper
    {
        SignInMessage GetSignInMessage(string id);
        void IssueLoginCookie(string id, string displayName);
        void RemovePartialLoginCookie();
        void SignInUser(string id, string displayName, string email);
        ActionResult SignOutUser();
        string GetClaimValue(string claimKey);
    }
}
