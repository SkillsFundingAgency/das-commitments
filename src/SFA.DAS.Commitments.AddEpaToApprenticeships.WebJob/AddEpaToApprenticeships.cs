using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Provider.Events.Api.Types;

namespace SFA.DAS.Commitments.AddEpaToApprenticeships.WebJob
{
    public class AddEpaToApprenticeships : IAddEpaToApprenticeships
    {
        private readonly ILog _logger;
        private readonly IAssessmentOrgs _assessmentOrgsService;
        private readonly IPaymentEvents _paymentEventsService;
        private readonly IAssessmentOrganisationRepository _assessmentOrganisationRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IJobProgressRepository _jobProgressRepository;

        private bool _assessmentOrganisationCacheUpdated;

        public AddEpaToApprenticeships(ILog logger,
            IAssessmentOrgs assessmentOrgsService,
            IPaymentEvents paymentEventsService,
            IAssessmentOrganisationRepository assessmentOrganisationRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IJobProgressRepository jobProgressRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(ILog));
            _assessmentOrgsService = assessmentOrgsService ?? throw new ArgumentNullException(nameof(assessmentOrgsService));
            _paymentEventsService = paymentEventsService ?? throw new ArgumentNullException(nameof(IPaymentEvents));
            _assessmentOrganisationRepository = assessmentOrganisationRepository ?? throw new ArgumentNullException(nameof(assessmentOrganisationRepository));
            _apprenticeshipRepository = apprenticeshipRepository ?? throw new ArgumentNullException(nameof(apprenticeshipRepository));
            _jobProgressRepository = jobProgressRepository ?? throw new ArgumentNullException(nameof(jobProgressRepository));
        }

        public async Task Update()
        {
            await UpdateApprenticeshipsWithEpaOrgIdFromSubmissionEvents();
        }

        private async Task UpdateApprenticeshipsWithEpaOrgIdFromSubmissionEvents()
        {
            long? pageLastId;
            var lastId = await _jobProgressRepository.Get_AddEpaToApprenticeships_LastSubmissionEventId() ?? 0;

            // instead of paging through, we only deal with the 1st page(s) and update the lastId
            PageOfResults<SubmissionEvent> page;
            do
            {
                page = await _paymentEventsService.GetSubmissionEvents(lastId);

                if (page.Items != null && page.Items.Any())
                    await UpdateCacheOfAssessmentOrganisations();

                pageLastId = await UpdateApprenticeshipsWithEpaOrgId(page.Items);
                if (pageLastId != null)
                {
                    await _jobProgressRepository.Set_AddEpaToApprenticeships_LastSubmissionEventId(pageLastId.Value);
                    lastId = pageLastId.Value;
                }

            } while (pageLastId.HasValue && page.TotalNumberOfPages > page.PageNumber);
        }

        private async Task<long?> UpdateApprenticeshipsWithEpaOrgId(IEnumerable<SubmissionEvent> submissionEvents)
        {
            foreach (var submissionEvent in submissionEvents)
            {
                if (!submissionEvent.ApprenticeshipId.HasValue)
                {
                    _logger.Info($"Ignoring SubmissionEvent '{submissionEvent.Id}' with no ApprenticheshipId");
                }
                else
                {
                    try
                    {
                        await _apprenticeshipRepository.UpdateApprenticeshipEpa(submissionEvent.ApprenticeshipId.Value, submissionEvent.EPAOrgId);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        _logger.Error(e, $"Attempt to set EPAOrgId for unknown apprenticeship with id {submissionEvent.ApprenticeshipId.Value}");
                    }
                }
            }

            return submissionEvents.LastOrDefault()?.Id;
        }

        private async Task UpdateCacheOfAssessmentOrganisations()
        {
            if (_assessmentOrganisationCacheUpdated)
                return;

            // fetch the highest EPAOrgId in our local cache of assessment organisations
            _logger.Info("Fetching all assessment orgs");

            var allOrganisationSummaries = await _assessmentOrgsService.All();

            // dev helpers:
            //var currentLongest = organisationSummaries.Max(o => o.Name.Length); // = 71
            //var orgs = JsonConvert.SerializeObject(allOrganisationSummaries);

            var latestCachedEPAOrgId = await _assessmentOrganisationRepository.GetLatestEpaOrgId();

            _logger.Info($"Latest EPAOrgId in cache is {latestCachedEPAOrgId ?? "N/A. Cache is Empty"}");

            // assumes summaries are returned ordered asc by Id
            var organisationSummariesToAdd = latestCachedEPAOrgId == null
                ? allOrganisationSummaries
                : allOrganisationSummaries.SkipWhile(os => os.Id != latestCachedEPAOrgId).Skip(1);

            if (!organisationSummariesToAdd.Any())
                return;

            // we could ditch the projection if we named the fields in the table the same as returned from the api
            var assessmentOrganisationsToAdd = organisationSummariesToAdd.Select(os => new AssessmentOrganisation { EPAOrgId = os.Id, Name = os.Name });

            await _assessmentOrganisationRepository.Add(assessmentOrganisationsToAdd);

            _assessmentOrganisationCacheUpdated = true;
        }
    }
}
