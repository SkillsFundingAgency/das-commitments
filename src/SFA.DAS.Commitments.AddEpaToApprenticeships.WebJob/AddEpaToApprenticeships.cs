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

            _logger.Info($"Using {_paymentEventsService.GetType().Name} for payment events service");
            _logger.Info($"Using {_assessmentOrganisationRepository.GetType().Name} for assessment organisation service");
        }

        public async Task Update()
        {
            await UpdateApprenticeshipsWithEpaOrgIdFromSubmissionEvents();
        }

        private async Task UpdateApprenticeshipsWithEpaOrgIdFromSubmissionEvents()
        {
            long? pageLastId;
            var lastId = await _jobProgressRepository.Get_AddEpaToApprenticeships_LastSubmissionEventId() ?? 0;
            _logger.Info($"Last SubmissionEventId processed by previous job run is {lastId}");

            // instead of paging through, we only deal with the 1st page(s) and update the lastId
            PageOfResults<SubmissionEvent> page;
            do
            {
                _logger.Info("Retrieving SubmissionEvents");
                page = await _paymentEventsService.GetSubmissionEvents(lastId);

                if (page.Items == null || !page.Items.Any())
                {
                    _logger.Info("No SubmissionEvents to process");
                    return;
                }
                _logger.Info($"Retrieved {page.Items.Length} SubmissionEvents");

                await UpdateCacheOfAssessmentOrganisations();

                pageLastId = await UpdateApprenticeshipsWithEpaOrgId(page.Items);
                if (pageLastId != null)
                {
                    _logger.Info($"Storing latest SubmissionEventId as {pageLastId.Value}");
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
                    _logger.Warn($"Ignoring SubmissionEvent '{submissionEvent.Id}' with no ApprenticheshipId");
                }
                else
                {
                    try
                    {
                        await _apprenticeshipRepository.UpdateApprenticeshipEpa(submissionEvent.ApprenticeshipId.Value, submissionEvent.EPAOrgId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Ignoring failed attempt to set EPAOrgId to '{submissionEvent.EPAOrgId}' for apprenticeship with id '{submissionEvent.ApprenticeshipId.Value}'\r\n");
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
            _logger.Info($"Fetched {allOrganisationSummaries.Count()} OrganisationSummaries");

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
            {
                _logger.Info("Organisation org cache is already up-to-date.");
                return;
            }

            // we could ditch the projection if we named the fields in the table the same as returned from the api
            var assessmentOrganisationsToAdd = organisationSummariesToAdd.Select(os => new AssessmentOrganisation { EPAOrgId = os.Id, Name = os.Name });

            _logger.Info($"Adding {assessmentOrganisationsToAdd.Count()} assessment orgs into cache");
            await _assessmentOrganisationRepository.Add(assessmentOrganisationsToAdd);

            _assessmentOrganisationCacheUpdated = true;
        }
    }
}
