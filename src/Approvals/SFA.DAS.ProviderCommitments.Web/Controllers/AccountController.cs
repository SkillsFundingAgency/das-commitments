using System.Web.Mvc;

namespace SFA.DAS.ProviderCommitments.Web.Controllers
{
    public class AccountController : Controller
    {
        [Route("~/signin", Name = "signin")]
        public void SignIn()
        {
        }

        [Route("~/signout", Name = "signout")]
        public void SignOut()
        {
        }

        public ActionResult SignOutCallback()
        {
            return HttpNotFound("Not implemented I'm afraid");
        }
    }
}