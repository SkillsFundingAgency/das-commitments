using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
            await UpdateCacheOfAssessmentOrganisationsAsync();

            await UpdateApprenticeshipsWithEPAOrgIdFromSubmissionEventsAsync();
        }

        private async Task UpdateApprenticeshipsWithEPAOrgIdFromSubmissionEventsAsync()
        {
            long? pageLastId;
            var lastId = await _jobProgressRepository.Get_AddEpaToApprenticeships_LastSubmissionEventIdAsync() ?? 0;

            // we could page through or only deal with the 1st page and update the lastId
            PageOfResults<SubmissionEvent> page;
            do
            {
                page = await _paymentEventsService.GetSubmissionEvents(lastId);

                pageLastId = await UpdateApprenticeshipsWithEPAOrgIdAsync(page.Items);
                if (pageLastId != null)
                {
                    await _jobProgressRepository.Set_AddEpaToApprenticeships_LastSubmissionEventIdAsync(pageLastId.Value);
                    lastId = pageLastId.Value;
                }

            } while (pageLastId.HasValue && page.TotalNumberOfPages > page.PageNumber);
        }

        private async Task<long?> UpdateApprenticeshipsWithEPAOrgIdAsync(IEnumerable<SubmissionEvent> submissionEvents)
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
                        await _apprenticeshipRepository.UpdateApprenticeshipEpaAsync(submissionEvent.ApprenticeshipId.Value, submissionEvent.EPAOrgId);
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        _logger.Error(e, $"Attempt to set EPAOrgId for unknown apprenticeship with id {submissionEvent.ApprenticeshipId.Value}");
                    }
                }
            }

            //todo: belongs in here?
            return submissionEvents.LastOrDefault()?.Id;
        }

        private async Task UpdateCacheOfAssessmentOrganisationsAsync()
        {
            // fetch the highest EPAOrgId in our local cache of assessment organisations
            _logger.Info("Fetching all assessment orgs");

            var allOrganisationSummaries = await _assessmentOrgsService.AllAsync();

            // dev helpers:
            //var currentLongest = organisationSummaries.Max(o => o.Name.Length); // = 71
            //var orgs = JsonConvert.SerializeObject(allOrganisationSummaries);

            var latestCachedEPAOrgId = await _assessmentOrganisationRepository.GetLatestEPAOrgIdAsync();

            _logger.Info($"Latest EPAOrgId in cache is {latestCachedEPAOrgId ?? "N/A. Cache is Empty"}");

            // assumes summaries are returned ordered asc by Id
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
