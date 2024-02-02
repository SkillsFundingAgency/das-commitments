using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "das-support-portal")]
    public class ApprenticeshipsController : Controller
    {
        private readonly IApprenticeshipsOrchestrator _orchestrator;

        public ApprenticeshipsController(IApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            _orchestrator = apprenticeshipsOrchestrator;
        }

        [Route("account/{hashedAccountId}/Cohort/{hashedCohortId}/", Name = "CohortDetails")]
        public async Task<ActionResult> CohortDetails(string hashedAccountId, string hashedCohortId)
        {
            if (string.IsNullOrWhiteSpace(hashedCohortId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetCommitmentDetails(hashedCohortId, hashedAccountId);
            return View(model);
        }

        [Route("account/{hashedAccountId}/Apprenticeship/{hashedApprenticeshipId}", Name = "ApprenticeshipDetails")]
        public async Task<ActionResult> Index(string hashedApprenticeshipId, string hashedAccountId)
        {
            if (string.IsNullOrWhiteSpace(hashedApprenticeshipId) || string.IsNullOrWhiteSpace(hashedAccountId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);
            return View(model);
        }

        [Route("search/{hashedAccountId}")]
        [HttpGet]
        public ActionResult Search(string hashedAccountId)
        {
            var uriString = $"/resource/apprenticeships/search/{hashedAccountId}";
            return View(new ApprenticeshipSearchQuery()
            {
                ResponseUrl = uriString
            });
        }

        [Route("search")]
        [HttpPost]
        public async Task<ActionResult> SearchRequest(string hashedAccountId, ApprenticeshipSearchType searchType, string searchTerm)
        {
            ApprenticeshipSearchQuery searchQuery = new ApprenticeshipSearchQuery()
            {
                HashedAccountId = hashedAccountId,
                SearchType = searchType,
                SearchTerm = searchTerm
            };

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
            var ulnSearchResult = await _orchestrator.GetApprenticeshipsByUln(searchQuery);
            if (ulnSearchResult.HasError)
            {
                searchQuery.ReponseMessages = ulnSearchResult.ReponseMessages;
                return View("Search", searchQuery);
            }

            ulnSearchResult.CurrentHashedAccountId = searchQuery.HashedAccountId;
            return View("UlnSearchSummary", ulnSearchResult);
        }
    }
}