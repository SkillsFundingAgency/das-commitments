﻿using System;
using SFA.DAS.Commitments.Support.SubSite.Enums;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    [System.Web.Mvc.Authorize(Roles = "das-support-portal")]
    public class ApprenticeshipsController : Controller
    {
        private readonly IApprenticeshipsOrchestrator _orchestrator;

        public ApprenticeshipsController(IApprenticeshipsOrchestrator apprenticeshipsOrchestrator)
        {
            _orchestrator = apprenticeshipsOrchestrator;
        }

        [Route("Apprenticeships/account/{hashedAccountId}/Cohort/{hashedCohortId}/", Name = "CohortDetails")]
        public async Task<ActionResult> CohortDetails(string hashedAccontId, string hashedCohortId)
        {
            if (string.IsNullOrWhiteSpace(hashedCohortId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetCommitmentDetails(hashedCohortId);
            return View(model);
        }

        [Route("Apprenticeships/account/{hashedAccountId}/Apprenticeship/{hashedApprenticeshipId}", Name = "ApprenticeshipDetails")]
        public async Task<ActionResult> Index(string hashedApprenticeshipId, string hashedAccountId)
        {
            if (string.IsNullOrWhiteSpace(hashedApprenticeshipId) || string.IsNullOrWhiteSpace(hashedAccountId))
            {
                return RedirectToAction(nameof(Search));
            }
            var model = await _orchestrator.GetApprenticeship(hashedApprenticeshipId, hashedAccountId);
            return View(model);
        }

        [Route("Apprenticeships/search/{hashedAccountId}")]
        [HttpGet]
        public async Task<ActionResult> Search(string hashedAccountId)
        {
            var uriString = $"/resource/apprenticeships/search/{hashedAccountId}";    
            return View(new ApprenticeshipSearchQuery()
            {
                ResponseUrl = uriString
            });
        }

        [Route("Apprenticeships/search")]
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