using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddEmptyCohortHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IEncodingService encodingService,
    ILogger<AddEmptyCohortHandler> logger,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<AddEmptyCohortCommand, AddCohortResult>
{
    public async Task<AddCohortResult> Handle(AddEmptyCohortCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

        var cohort = await cohortDomainService.CreateEmptyCohort(command.ProviderId,
            command.AccountId,
            command.AccountLegalEntityId,
            command.UserInfo,
            cancellationToken);

        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync(cancellationToken);

        //this encoding and re-save could be removed and put elsewhere
        cohort.Reference = encodingService.Encode(cohort.Id, EncodingType.CohortReference);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Saved empty cohort. Provider: {ProviderId} Account-Legal-Entity:{AccountLegalEntityId} Commitment-Id:{CohortId}", command.ProviderId, command.AccountLegalEntityId, cohort.Id);

        return new AddCohortResult
        {
            Id = cohort.Id,
            Reference = cohort.Reference
        };
    }
}