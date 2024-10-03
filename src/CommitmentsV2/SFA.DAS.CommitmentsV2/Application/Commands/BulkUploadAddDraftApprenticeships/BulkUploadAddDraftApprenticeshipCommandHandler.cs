using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;

public class BulkUploadAddDraftApprenticeshipCommandHandler(
    IModelMapper draftApprenticeshipDetailsMapper,
    ICohortDomainService cohortDomainService,
    ProviderCommitmentsDbContext providerCommitmentsDbContext,
    IEncodingService encodingService)
    : IRequestHandler<BulkUploadAddDraftApprenticeshipsCommand, GetBulkUploadAddDraftApprenticeshipsResponse>
{
    public async Task<GetBulkUploadAddDraftApprenticeshipsResponse> Handle(BulkUploadAddDraftApprenticeshipsCommand request, CancellationToken cancellationToken)
    {
        var draftApprenticeships = await draftApprenticeshipDetailsMapper.Map<List<DraftApprenticeshipDetails>>(request);
        var cohorts = await cohortDomainService.AddDraftApprenticeships(draftApprenticeships,
            request.BulkUploadDraftApprenticeships,
            request.ProviderId,
            request.UserInfo,
            cancellationToken);

        await providerCommitmentsDbContext.SaveChangesAsync(cancellationToken);

        await UpdateCohortReferences(cohorts);

        if (request.LogId != null)
        {
            var fileUploadLog = providerCommitmentsDbContext.FileUploadLogs.First(x => x.Id.Equals(request.LogId.Value));
            
            fileUploadLog.ProviderAction = request.ProviderAction;
            fileUploadLog.CompletedOn = DateTime.UtcNow;
            
            foreach (var cohort in cohorts)
            {
                fileUploadLog.CohortLogs.Add(new FileUploadCohortLog { CommitmentId = cohort.Id, RowCount = cohort.DraftApprenticeshipCount });
            }
        }

        var cohortSummaryForBulkUpload = cohorts.Select(cohort => new BulkUploadAddDraftApprenticeshipsResponse
        {
            CohortReference = cohort.Reference,
            NumberOfApprenticeships = cohort.Apprenticeships.Count,
            EmployerName = cohort.AccountLegalEntity.Name
        });

        return new GetBulkUploadAddDraftApprenticeshipsResponse { BulkUploadAddDraftApprenticeshipsResponse = cohortSummaryForBulkUpload };
    }

    private async Task UpdateCohortReferences(IEnumerable<Cohort> cohorts)
    {
        var anyNewCohorts = false;
        
        foreach (var cohort in cohorts.Where(x => string.IsNullOrWhiteSpace(x.Reference)))
        {
            cohort.Reference = encodingService.Encode(cohort.Id, EncodingType.CohortReference);
            anyNewCohorts = true;
        }

        if (anyNewCohorts)
        {
            // Another save for cohort references
            await providerCommitmentsDbContext.SaveChangesAsync();
        }
    }
}