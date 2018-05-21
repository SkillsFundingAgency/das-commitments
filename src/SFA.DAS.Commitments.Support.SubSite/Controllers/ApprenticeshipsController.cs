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
                    return await UlnSearch(searchQuery);

                case ApprenticeshipSearchType.SearchByCohort:
                    return CohortSearch(searchQuery);
            }

            return View(searchQuery);
        }

        public async Task<ActionResult> Index(string Id, string accountId)
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(accountId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetApprenticeship(Id, accountId);
            return View(model);
        }

        private async Task<ActionResult> UlnSearch(ApprenticeshipSearchQuery searchQuery)
        {
            var searchResult = await _orchestrator.GetApprenticeshipsByUln(searchQuery);
            if (searchResult.HasError)
            {
                searchQuery.ErrorMessages = searchResult.ReponseMessages;
                return View("Search", searchQuery);
            }
            return View("UlnSearchSummary", searchResult);
        }

        private ActionResult CohortSearch(ApprenticeshipSearchQuery searchQuery)
        {
            throw new NotImplementedException();
        }



    }
}