using System.Web.Mvc;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderCommitments.Web.Models;

namespace SFA.DAS.ProviderCommitments.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILog _logger;

        public HomeController(ILog logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            _logger.Info("Calling Index View on HomeController");
            return View();
        }

        [Authorize]
        public ActionResult Secured()
        {
            _logger.Info("Calling Index View on HomeController");
            var securedModel = new SecuredViewModel
            {
                FirstName = "Bill",
                LastName = "Bailey",
                EmailAddress = "BillBailey@gmail.com"
            };

            return View(securedModel);
        }

        [Route("~/terms", Name = "terms")]
        public ActionResult Terms()
        {
            return View();
        }

        [Route("privacy", Name = "privacy")]
        public ActionResult Privacy()
        {
            return View();
        }
    }
}