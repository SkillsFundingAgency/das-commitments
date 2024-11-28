using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddCohortWithOtherPartyHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IEncodingService encodingService,
    ILogger<AddCohortWithOtherPartyHandler> logger,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<AddCohortWithOtherPartyCommand, AddCohortResult>
{
    public async Task<AddCohortResult> Handle(AddCohortWithOtherPartyCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

        var cohort = await cohortDomainService.CreateCohortWithOtherParty(command.ProviderId,
            command.AccountId,
            command.AccountLegalEntityId,
            command.TransferSenderId,
            command.PledgeApplicationId,
            command.Message,
            command.UserInfo,
            cancellationToken);

        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync(cancellationToken);

        //this encoding and re-save could be removed and put elsewhere
        cohort.Reference = encodingService.Encode(cohort.Id, EncodingType.CohortReference);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Saved cohort with other party. Provider: {ProviderId} Account-Legal-Entity:{AccountLegalEntityId} Commitment-Id:{CohortId}", command.ProviderId, command.AccountLegalEntityId, cohort.Id);

        return new AddCohortResult
        {
            Id = cohort.Id,
            Reference = cohort.Reference
        };
    }
}