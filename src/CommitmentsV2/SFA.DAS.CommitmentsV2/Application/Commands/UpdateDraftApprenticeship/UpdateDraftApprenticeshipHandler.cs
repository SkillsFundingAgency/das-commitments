using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;

public class UpdateDraftApprenticeshipHandler(
    ILogger<UpdateDraftApprenticeshipHandler> logger,
    IOldMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
    ICohortDomainService cohortDomainService)
    : IRequestHandler<UpdateDraftApprenticeshipCommand, UpdateDraftApprenticeshipResponse>
{
    public async Task<UpdateDraftApprenticeshipResponse> Handle(UpdateDraftApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        var draftApprenticeshipDetails = await draftApprenticeshipDetailsMapper.Map(command);

        await cohortDomainService.UpdateDraftApprenticeship(command.CohortId, draftApprenticeshipDetails, command.UserInfo, command.RequestingParty, command.MinimumAgeAtApprenticeshipStart, command.MaximumAgeAtApprenticeshipStart, cancellationToken);

        logger.LogInformation("Saved cohort. Reservation-Id:{ReservationId} Commitment-Id:{CohortId} Apprenticeship:{ApprenticeshipId}", command.ReservationId, command.CohortId, command.ApprenticeshipId);

        return new UpdateDraftApprenticeshipResponse
        {
            Id = command.CohortId,
            ApprenticeshipId = command.ApprenticeshipId
        };
    }
}