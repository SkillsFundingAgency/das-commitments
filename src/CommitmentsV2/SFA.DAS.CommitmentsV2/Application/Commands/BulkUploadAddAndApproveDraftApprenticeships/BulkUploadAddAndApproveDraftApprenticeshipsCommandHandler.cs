using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;

public class BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler(
    ILogger<BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler> logger,
    ICohortDomainService cohortDomainService,
    IMediator mediator,
    IProviderCommitmentsDbContext providerDbContext)
    : IRequestHandler<BulkUploadAddAndApproveDraftApprenticeshipsCommand, BulkUploadAddAndApproveDraftApprenticeshipsResponse>
{
    public async Task<BulkUploadAddAndApproveDraftApprenticeshipsResponse> Handle(BulkUploadAddAndApproveDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
    {
        var results = await mediator.Send(new BulkUploadAddDraftApprenticeshipsCommand
        {
            UserInfo = request.UserInfo, 
            BulkUploadDraftApprenticeships = request.BulkUploadDraftApprenticeships,
            ProviderId = request.ProviderId, 
            LogId = request.LogId, 
            ProviderAction = "SaveAndApprove"
        }, cancellationToken);

        foreach (var result in results.BulkUploadAddDraftApprenticeshipsResponse)
        {
            var cohort = await providerDbContext.Cohorts.SingleAsync(c => c.Reference == result.CohortReference, cancellationToken: cancellationToken);
            
            await cohortDomainService.ApproveCohort(cohort.Id, "", request.UserInfo, Party.Provider, cancellationToken);
            
            logger.LogInformation("Bulk upload - Added and Approved  draft apprenticeship. Commitment-Reference:{CohortReference} Commitment-Id:{CohortId}", cohort.Reference, cohort.Id);
        }

        return new BulkUploadAddAndApproveDraftApprenticeshipsResponse()
        {
            BulkUploadAddAndApproveDraftApprenticeshipResponse = results.BulkUploadAddDraftApprenticeshipsResponse
        };
    }
}