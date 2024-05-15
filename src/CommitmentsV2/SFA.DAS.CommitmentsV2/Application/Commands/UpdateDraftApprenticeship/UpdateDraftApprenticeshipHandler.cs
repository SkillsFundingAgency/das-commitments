using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateDraftApprenticeship;

public class UpdateDraftApprenticeshipHandler : IRequestHandler<UpdateDraftApprenticeshipCommand, UpdateDraftApprenticeshipResponse>
{
    private readonly ILogger<UpdateDraftApprenticeshipHandler> _logger;
    private readonly IOldMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
    private readonly ICohortDomainService _cohortDomainService;

    public UpdateDraftApprenticeshipHandler(
        ILogger<UpdateDraftApprenticeshipHandler> logger,
        IOldMapper<UpdateDraftApprenticeshipCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
        ICohortDomainService cohortDomainService)
    {
        _logger = logger;
        _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
        _cohortDomainService = cohortDomainService;
    }

    public async Task<UpdateDraftApprenticeshipResponse> Handle(UpdateDraftApprenticeshipCommand command, CancellationToken cancellationToken)
    {
        var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(command);

        await _cohortDomainService.UpdateDraftApprenticeship(command.CohortId, draftApprenticeshipDetails, command.UserInfo, command.RequestingParty, cancellationToken);

        _logger.LogInformation("Saved cohort. Reservation-Id:{ReservationId} Commitment-Id:{CohortId} Apprenticeship:{ApprenticeshipId}", command.ReservationId, command.CohortId, command.ApprenticeshipId);

        var response = new UpdateDraftApprenticeshipResponse
        {
            Id = command.CohortId,
            ApprenticeshipId = command.ApprenticeshipId
        };

        return response;
    }
}