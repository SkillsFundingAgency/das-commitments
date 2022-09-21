using System;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations
{
    public class UpdateCacheOfAssessmentOrganisationsCommandHandler : IRequestHandler<UpdateCacheOfAssessmentOrganisationsCommand>
    {
        private IApprovalsOuterApiClient _outerApiClient;
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

        public async Task<Unit> Handle(UpdateCacheOfAssessmentOrganisationsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Fetching all assessment orgs");
            var epaoResponse = await _outerApiClient.Get<EpaoResponse>(new GetEpaoOrganisationsRequest());

            var allOrganisationSummaries = epaoResponse.Epaos;

            _logger.LogInformation($"Fetched {allOrganisationSummaries.Count()} OrganisationSummaries");

            var latestCachedEPAOrgId = _providerDbContext.Value.AssessmentOrganisations.Max(x => x.EpaOrgId);
            _logger.LogInformation($"Latest EPAOrgId in cache is {latestCachedEPAOrgId ?? "N/A. Cache is Empty"}");

            // assumes summaries are returned ordered asc by Id
            var organisationSummariesToAdd = latestCachedEPAOrgId == null
                ? allOrganisationSummaries
                : allOrganisationSummaries.SkipWhile(os => os.Id != latestCachedEPAOrgId).Skip(1);

            if (!organisationSummariesToAdd.Any())
            {
                _logger.LogInformation("Organisation org cache is already up-to-date.");
                return Unit.Value;
            }

            var assessmentOrganisationsToAdd = organisationSummariesToAdd.Select(os => new AssessmentOrganisation { EpaOrgId = os.Id, Name = os.Name });

            _logger.LogInformation($"Adding {assessmentOrganisationsToAdd.Count()} assessment orgs into cache");
            await _providerDbContext.Value.AssessmentOrganisations.AddRangeAsync(assessmentOrganisationsToAdd);

            await _providerDbContext.Value.SaveChangesAsync();

            return Unit.Value;
        }
    }
}
