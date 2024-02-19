using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations
{
    public class
        UpdateCacheOfAssessmentOrganisationsCommandHandler : IRequestHandler<
            UpdateCacheOfAssessmentOrganisationsCommand>
    {
        private readonly IApprovalsOuterApiClient _outerApiClient;
        private readonly Lazy<ProviderCommitmentsDbContext> _providerDbContext;
        private readonly ILogger<UpdateCacheOfAssessmentOrganisationsCommandHandler> _logger;

        public UpdateCacheOfAssessmentOrganisationsCommandHandler(IApprovalsOuterApiClient outerApiClient,
            Lazy<ProviderCommitmentsDbContext> providerDbContext,
            ILogger<UpdateCacheOfAssessmentOrganisationsCommandHandler> logger)
        {
            _outerApiClient = outerApiClient;
            _providerDbContext = providerDbContext;
            _logger = logger;
        }

        public async Task Handle(UpdateCacheOfAssessmentOrganisationsCommand request, CancellationToken cancellationToken)
        {
            // .ToList() utilised to prevent possible multiple enumerations ...
            
            _logger.LogInformation("Fetching all assessment orgs");
            var epaoResponse = await _outerApiClient.Get<EpaoResponse>(new GetEpaoOrganisationsRequest());

            var allOrganisationSummaries = epaoResponse.Epaos.ToList();

            _logger.LogInformation($"Fetched {allOrganisationSummaries.Count()} OrganisationSummaries");

            var latestCachedEPAOrgId = _providerDbContext.Value.AssessmentOrganisations.Select(x => x.EpaOrgId)
                .OrderByDescending(x => x).FirstOrDefault();

            _logger.LogInformation($"Latest EPAOrgId in cache is {latestCachedEPAOrgId ?? "N/A. Cache is Empty"}");

            // assumes summaries are returned ordered asc by Id
            var organisationSummariesToAdd = latestCachedEPAOrgId == null
                ? allOrganisationSummaries
                : allOrganisationSummaries.SkipWhile(os => os.Id != latestCachedEPAOrgId).Skip(1)
                    .ToList();

            if (!organisationSummariesToAdd.Any())
            {
                _logger.LogInformation("Organisation org cache is already up-to-date.");
                return;
            }

            var assessmentOrganisationsToAdd = organisationSummariesToAdd
                .Select(os => new AssessmentOrganisation { EpaOrgId = os.Id, Name = os.Name })
                .ToList();

            _logger.LogInformation($"Adding {assessmentOrganisationsToAdd.Count} assessment orgs into cache");
            
            await _providerDbContext.Value.AssessmentOrganisations.AddRangeAsync(assessmentOrganisationsToAdd, cancellationToken);

            await _providerDbContext.Value.SaveChangesAsync(cancellationToken);
        }
    }
}