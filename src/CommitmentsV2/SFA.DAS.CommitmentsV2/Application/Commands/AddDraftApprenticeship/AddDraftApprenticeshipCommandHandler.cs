using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddDraftApprenticeship;

public class AddDraftApprenticeshipCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<AddDraftApprenticeshipCommandHandler> logger,
    IOldMapper<AddDraftApprenticeshipCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<AddDraftApprenticeshipCommand, AddDraftApprenticeshipResult>
{
    public async Task<AddDraftApprenticeshipResult> Handle(AddDraftApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        var db = dbContext.Value;
        var draftApprenticeshipDetails = await draftApprenticeshipDetailsMapper.Map(command);
        var draftApprenticeship = await cohortDomainService.AddDraftApprenticeship(
            command.ProviderId,
            command.CohortId, 
            draftApprenticeshipDetails,
            command.UserInfo,
            command.MinimumAgeAtApprenticeshipStart,
            command.MaximumAgeAtApprenticeshipStart,
            command.RequestingParty,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Added draft apprenticeship. Reservation-Id:{ReservationId} Commitment-Id:{CohortId} Apprenticeship-Id:{DraftApprenticeshipId}",
            command.ReservationId,
            command.CohortId,
            draftApprenticeship.Id
        );

        return new AddDraftApprenticeshipResult
        {
            Id = draftApprenticeship.Id
        };
    }
}