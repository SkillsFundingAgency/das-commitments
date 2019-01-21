using System.Web.Mvc;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.ProviderCommitments.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        public HomeController(ILogger logger)
        {
            _logger = logger;
        }

        public ActionResult Index()
        {
            _logger.LogInformation("Calling Index View on HomeController");
            return View();
        }

    }
}