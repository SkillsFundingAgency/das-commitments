using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System.Threading.Tasks;
using System.Web.Mvc;
using Mvc = System.Web.Mvc;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    [Authorize]
    public class ApprenticeshipsController : Controller
    {
        private readonly IApprenticeshipsOrchestrator _orchestrator;

        public ApprenticeshipsController(IApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            _orchestrator = apprenticeshipsOrchestrator;
        }

        [Mvc.Route("Apprenticeships/Cohort/{hashedCohortId}/", Name = "CohortDetails")]
        public async Task<ActionResult> CohortDetails(string hashedCohortId)
        {
            if (string.IsNullOrWhiteSpace(hashedCohortId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetCommitmentDetails(hashedCohortId);
            return View(model);
        }

        [Mvc.Route("Apprenticeships/{hashedApprenticeshipId}/account/{hashedAccountId}", Name = "ApprenticeshipDetails")]
        public async Task<ActionResult> Index(string hashedApprenticeshipId, string hashedAccountId)
        {
            if (string.IsNullOrWhiteSpace(hashedApprenticeshipId) || string.IsNullOrWhiteSpace(hashedAccountId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);
            return View(model);
        }

        public async Task<ActionResult> Search(ApprenticeshipSearchQuery searchQuery)
        {
            if (string.IsNullOrWhiteSpace(searchQuery.SearchTerm))
            {
                return View(searchQuery);
            }

            switch (searchQuery.SearchType)
            {
                case ApprenticeshipSearchType.SearchByUln:
                    return await UlnSearch(searchQuery);

                case ApprenticeshipSearchType.SearchByCohort:
                    return await CohortSearch(searchQuery);
            }

            return View(searchQuery);
        }
        private async Task<ActionResult> CohortSearch(ApprenticeshipSearchQuery searchQuery)
        {
            var cohortSearchResult = await _orchestrator.GetCommitmentSummary(searchQuery);
            if (cohortSearchResult.HasError)
            {
                searchQuery.ReponseMessages = cohortSearchResult.ReponseMessages;
                return View("Search", searchQuery);
            }
            return View("CohortSearchSummary", cohortSearchResult);
        }

        private async Task<ActionResult> UlnSearch(ApprenticeshipSearchQuery searchQuery)
        {
            var unlSearchResult = await _orchestrator.GetApprenticeshipsByUln(searchQuery);
            if (unlSearchResult.HasError)
            {
                searchQuery.ReponseMessages = unlSearchResult.ReponseMessages;
                return View("Search", searchQuery);
            }
            return View("UlnSearchSummary", unlSearchResult);
        }
    }
}