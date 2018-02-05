using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Types.AssessmentOrgs;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob
{
    public class AddEpaToApprenticeships : IAddEpaToApprenticeships
    {
        private readonly ILog _logger;
        private readonly IAssessmentOrgs _assessmentOrgsService;
        private readonly IPaymentEvents _paymentEventsService;
        private readonly IAssessmentOrganisationRepository _assessmentOrganisationRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public AddEpaToApprenticeships(ILog logger,
            IAssessmentOrgs assessmentOrgsService,
            IPaymentEvents paymentEventsService,
            IAssessmentOrganisationRepository assessmentOrganisationRepository,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILog));
            _assessmentOrgsService = assessmentOrgsService ?? throw new ArgumentNullException(nameof(assessmentOrgsService));
            _paymentEventsService = paymentEventsService ?? throw new ArgumentNullException(nameof(IPaymentEvents));
            _assessmentOrganisationRepository = assessmentOrganisationRepository ?? throw new ArgumentNullException(nameof(assessmentOrganisationRepository));
            _apprenticeshipRepository = apprenticeshipRepository ?? throw new ArgumentNullException(nameof(apprenticeshipRepository));
        }

        public async Task Update()
        {
            await UpdateCacheOfAssessmentOrganisationsAsync(); //todo: rename, not actually a cache

            await UpdateApprenticeshipsWithEPAOrgIdFromSubmissionEventsAsync();
        }

        private async Task UpdateApprenticeshipsWithEPAOrgIdFromSubmissionEventsAsync()
        {
            long lastId = 0;

            var page = await _paymentEventsService.GetSubmissionEvents(lastId);

            foreach (var submissionEvent in page.Items)
            {
                try
                {
                    //todo: do we need to handle events with null apprenticeship ids?
                    await _apprenticeshipRepository.UpdateApprenticeshipEPAAsync(submissionEvent.ApprenticeshipId.Value, submissionEvent.EPAOrgId);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    _logger.Error(e, $"Attempt to set EPAOrdId for unknown apprenticeship with id {submissionEvent.ApprenticeshipId.Value}");
                }
            }
        }

        private async Task UpdateCacheOfAssessmentOrganisationsAsync()
        {
            //todo: add FK relationship?
            //todo: when merging in repos, how to handle updated nuget packages,
            // e.g. we need an updated SFA.DAS.Provider.Events.Api.Client here
            // is there a nice way to do it? do we have to merge code that updates client first
            // then update reference in consumers, then merge consumers - seems a bit naff

            // fetch the highest EPAOrgId in our local cache of assessment organisations
            _logger.Info("Fetching all assessment orgs");

            var allOrganisationSummaries = await _assessmentOrgsService.AllAsync();
            //var currentLongest = organisationSummaries.Max(o => o.Name.Length); // = 71

            // assumes summaries are returned ordered asc by Id

            //var latestCachedEPAOrgId = await _assessmentOrganisationRepository.GetLatestEPAOrgId() ?? organisationSummaries.First().Id;

            var latestCachedEPAOrgId = await _assessmentOrganisationRepository.GetLatestEPAOrgId();

            _logger.Info($"Latest EPAOrgId in cache is {latestCachedEPAOrgId ?? "N/A. Cache is Empty"}");

            var organisationSummariesToAdd = latestCachedEPAOrgId == null
                ? allOrganisationSummaries
                : allOrganisationSummaries.SkipWhile(os => os.Id != latestCachedEPAOrgId).Skip(1);

            if (!organisationSummariesToAdd.Any())
                return;

            // we could ditch the projection if we named the fields in the table the same as returned from the api
            var assessmentOrganisationsToAdd = organisationSummariesToAdd.Select(os => new AssessmentOrganisation { EPAOrgId = os.Id, Name = os.Name });

            await _assessmentOrganisationRepository.AddAsync(assessmentOrganisationsToAdd);
        }
    }
}
