using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Controllers
{
    [Authorize]
    public class CommitmentController : Controller
    {
        private readonly CommitmentOrchestrator _commitmentOrchestrator;

        public CommitmentController(CommitmentOrchestrator commitmentOrchestrator)
        {
            if (commitmentOrchestrator == null)
                throw new ArgumentNullException(nameof(commitmentOrchestrator));
            _commitmentOrchestrator = commitmentOrchestrator;
        }
        
        public async Task<ActionResult> Index(long providerId)
        {
            var model = await _commitmentOrchestrator.GetAll(providerId);

            return View(model);
        }

        public async Task<ActionResult> Details(long providerId, long commitmentId)
        {
            var model = await _commitmentOrchestrator.Get(providerId, commitmentId);

            return View(model);
        }

        public async Task<ActionResult> Edit(long providerId, long commitmentId, long apprenticeshipId)
        {
            var model = await _commitmentOrchestrator.GetApprenticeship(providerId, commitmentId, apprenticeshipId);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Update(ApprenticeshipViewModel apprenticeship)
        {
            await _commitmentOrchestrator.UpdateApprenticeship(apprenticeship);

            return RedirectToAction("Details", new {providerId = apprenticeship.ProviderId, commitmentId = apprenticeship.CommitmentId });
        }

        public async Task<ActionResult> Create(long providerId, long commitmentId)
        {
            var model = await _commitmentOrchestrator.GetApprenticeship(providerId, commitmentId);

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Create(ApprenticeshipViewModel apprenticeship)
        {
            await _commitmentOrchestrator.CreateApprenticeship(apprenticeship);

            return RedirectToAction("Details", new { providerId = apprenticeship.ProviderId, commitmentId = apprenticeship.CommitmentId });
        }
    }
}