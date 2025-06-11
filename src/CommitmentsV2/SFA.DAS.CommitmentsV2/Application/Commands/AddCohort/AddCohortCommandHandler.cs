using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddCohortCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IEncodingService encodingService,
    ILogger<AddCohortCommandHandler> logger,
    IOldMapper<AddCohortCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<AddCohortCommand, AddCohortResult>
{
    public async Task<AddCohortResult> Handle(AddCohortCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;

        var draftApprenticeshipDetails = await draftApprenticeshipDetailsMapper.Map(command);

        var cohort = await cohortDomainService.CreateCohort(command.ProviderId,
            command.AccountId,
            command.AccountLegalEntityId,
            command.TransferSenderId,
            command.PledgeApplicationId,
            draftApprenticeshipDetails,
            command.UserInfo,
            command.RequestingParty,
            cancellationToken);

        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync(cancellationToken);

        //this encoding and re-save could be removed and put elsewhere
        cohort.Reference = encodingService.Encode(cohort.Id, EncodingType.CohortReference);
        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Saved cohort. Provider: {ProviderId} Account-Legal-Entity:{AccountLegalEntityId} Reservation-Id:{ReservationId} Commitment-Id:{Id} Apprenticeship:{ApprenticeshipId}",
            command.ProviderId,
            command.AccountLegalEntityId,
            command.ReservationId,
            cohort.Id,
            cohort.Apprenticeships?.FirstOrDefault()?.Id
        );

        return new AddCohortResult
        {
            Id = cohort.Id,
            Reference = cohort.Reference
        };
    }
}