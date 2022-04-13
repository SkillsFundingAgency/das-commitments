using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Commitments.Support.SubSite.Core.Enums;
using SFA.DAS.Commitments.Support.SubSite.Core.Models;
using SFA.DAS.Commitments.Support.SubSite.Core.Orchestrators;

namespace SFA.DAS.Commitments.Support.SubSite.Core.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApprenticeshipsController : Controller
    {
        private readonly IApprenticeshipsOrchestrator _orchestrator;

        public ApprenticeshipsController(IApprenticeshipsOrchestrator orchestrator)
        {
            _orchestrator = orchestrator;
        }

        [Route("search/{hashedAccountId}")]
        [HttpGet]
        public async Task<ActionResult> Search(string hashedAccountId)
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
