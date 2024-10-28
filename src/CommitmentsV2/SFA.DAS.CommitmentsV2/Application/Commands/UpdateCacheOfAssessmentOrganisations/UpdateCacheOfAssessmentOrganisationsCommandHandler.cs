using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations;

public class
    UpdateCacheOfAssessmentOrganisationsCommandHandler(
        IApprovalsOuterApiClient outerApiClient,
        Lazy<ProviderCommitmentsDbContext> providerDbContext,
        ILogger<UpdateCacheOfAssessmentOrganisationsCommandHandler> logger)
    : IRequestHandler<UpdateCacheOfAssessmentOrganisationsCommand>
{
    public async Task Handle(UpdateCacheOfAssessmentOrganisationsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Fetching all assessment orgs");
        
        var epaoResponse = await outerApiClient.Get<EpaoResponse>(new GetEpaoOrganisationsRequest());

        // .ToList() utilised to prevent possible multiple enumerations ...
        var allOrganisationSummaries = epaoResponse.Epaos.ToList();

        logger.LogInformation("Fetched {Count} OrganisationSummaries", allOrganisationSummaries.Count);

        var latestCachedEPAOrgId = providerDbContext.Value.AssessmentOrganisations.Select(x => x.EpaOrgId)
            .OrderByDescending(x => x).FirstOrDefault();

        logger.LogInformation("Latest EPAOrgId in cache is {latestCachedEPAOrgId}", latestCachedEPAOrgId ?? "N/A. Cache is Empty");

        var organisationSummariesToAdd = latestCachedEPAOrgId == null
            ? allOrganisationSummaries
            : allOrganisationSummaries
                .OrderBy(x=> x.Id)
                .SkipWhile(os => os.Id != latestCachedEPAOrgId).Skip(1)
                .ToList();

        if (!organisationSummariesToAdd.Any())
        {
            logger.LogInformation("Organisation org cache is already up-to-date.");
            return;
        }

        var assessmentOrganisationsToAdd = organisationSummariesToAdd
            .Select(os => new AssessmentOrganisation { EpaOrgId = os.Id, Name = os.Name })
            .ToList();

        logger.LogInformation("Adding {Count} assessment orgs into cache", assessmentOrganisationsToAdd.Count);
            
        await providerDbContext.Value.AssessmentOrganisations.AddRangeAsync(assessmentOrganisationsToAdd, cancellationToken);

        await providerDbContext.Value.SaveChangesAsync(cancellationToken);
    }
}