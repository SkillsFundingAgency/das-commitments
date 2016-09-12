using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Controllers
{
    public class TasksController : Controller
    {
        private readonly TaskOrchestrator _taskOrchestrator;

        public TasksController(TaskOrchestrator taskOrchestrator)
        {
            if (taskOrchestrator == null)
                throw new ArgumentNullException(nameof(taskOrchestrator));
            _taskOrchestrator = taskOrchestrator;
        }

        // GET: Tasks
        public async Task<ActionResult> Index(long providerId)
        {
            var model = await _taskOrchestrator.GetAll(providerId);

            return View(model);
        }
    }
}