using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.Reservations.Api.Types;
using ReservationValidationResult = SFA.DAS.Reservations.Api.Types.ReservationValidationResult;

namespace SFA.DAS.CommitmentsV2.Services;

public class ReservationValidationService(
    IReservationsApiClient apiClient,
    IOldMapper<ReservationValidationRequest, ReservationValidationMessage> requestMapper,
    IOldMapper<ReservationValidationResult, Domain.Entities.Reservations.ReservationValidationResult> resultMapper)
    : IReservationValidationService
{
    public async Task<Domain.Entities.Reservations.ReservationValidationResult> Validate(ReservationValidationRequest request, CancellationToken cancellationToken)
    {
        var mappedRequest = await requestMapper.Map(request);
        var result = await apiClient.ValidateReservation(mappedRequest, cancellationToken);
        
        return await resultMapper.Map(result);
    }
}