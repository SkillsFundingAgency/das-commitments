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
using SFA.DAS.Commitments.Support.SubSite.Enums;
using Mvc = System.Web.Mvc;

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
                    return await CohortSearch(searchQuery);
            }

            return View(searchQuery);
        }

        [Mvc.Route("Apprenticeships/{Id}/account/{accountId}", Name = "ApprenticeshipDetails")]
        public async Task<ActionResult> Index(string Id, string accountId)
        {
            if (string.IsNullOrWhiteSpace(Id) || string.IsNullOrWhiteSpace(accountId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetApprenticeship(Id, accountId);
            return View(model);
        }

        [Mvc.Route("Apprenticeships/Cohort/{cohortId}/", Name = "CohortDetails")]
        public async Task<ActionResult> CohortDetails(string cohortId)
        {
            if (string.IsNullOrWhiteSpace(cohortId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetCommitmentDetails(cohortId);
            return View(model);
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

    }
}