using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.ProviderApprenticeshipsService.Web.Authentication;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOwinWrapper _owinWrapper;
        private readonly HomeOrchestrator _homeOrchestrator;

        public HomeController(IOwinWrapper owinWrapper, HomeOrchestrator homeOrchestrator)
        {
            if (owinWrapper == null)
                throw new ArgumentNullException(nameof(owinWrapper));
            if (homeOrchestrator == null)
                throw new ArgumentNullException(nameof(homeOrchestrator));
            _owinWrapper = owinWrapper;
            _homeOrchestrator = homeOrchestrator;
        }

        [Authorize]
        public async Task<ActionResult> Index(long? providerId)
        {
            if (!providerId.HasValue)
            {
                var users = await _homeOrchestrator.GetUsers();

                var userId = _owinWrapper.GetClaimValue("sub");

                var user = users.AvailableUsers.SingleOrDefault(x => x.UserId == userId);

                providerId = user.ProviderId;
            }

            var model = new HomeIndexModel
            {
                ProviderId = providerId.Value
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult SignInUser(string selectedUserId, SignInUserViewModel model)
        {

            var selected = model.AvailableUsers.FirstOrDefault(x => selectedUserId == x.UserId);

            if (selected != null)
            {
                LoginUser(selected.UserId, selected.FirstName, selected.LastName);
            }

            return RedirectToAction("Index", new {providerId = selected.ProviderId});
        }
        
        public async Task<ActionResult> FakeUserSignIn()
        {
            var users = await _homeOrchestrator.GetUsers();

            return View(users);
        }

        public ActionResult SignOut()
        {
            return _owinWrapper.SignOutUser();
        }

        private void LoginUser(string id, string firstName, string lastName)
        {
            var displayName = $"{firstName} {lastName}";
            _owinWrapper.SignInUser(id, displayName, $"{firstName.Trim()}.{lastName.Trim()}@test.local");

            _owinWrapper.IssueLoginCookie(id, displayName);

            _owinWrapper.RemovePartialLoginCookie();
        }
    }
}