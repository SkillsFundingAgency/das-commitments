using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.Support.Shared;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    public class ApprenticeshipsController : Controller
    {
        private readonly IApprenticeshipsOrchestrator _orchestrator;

        public ApprenticeshipsController(IApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            _orchestrator = apprenticeshipsOrchestrator;
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
                    return await UlnSearch( searchQuery);

                case ApprenticeshipSearchType.SearchByCohort:
                    return await CohortSearch(searchQuery);
            }

            return View(searchQuery);
        }

        private async Task<ActionResult> UlnSearch(ApprenticeshipSearchQuery searchQuery)
        {
            var searchResult = await _orchestrator.GetApprenticeshipsByUln(searchQuery);
            if (searchResult.HasError)
            {
                searchQuery.ErrorMessages = searchResult.ErrorMessages;
                return View("Search", searchQuery);
            }
            return View("ApprenticeshipsUlnSearchSummary", searchResult);
        }

        private async Task<ActionResult> CohortSearch(ApprenticeshipSearchQuery searchQuery)
        {
            var searchResult = await _orchestrator.GetApprenticeshipsByUln(searchQuery);
            if (searchResult.HasError)
            {
                searchQuery.ErrorMessages = searchResult.ErrorMessages;
                return View(searchQuery);
            }
            return View("ApprenticeshipsCohortSearchSummary", searchResult);
        }

    }
}