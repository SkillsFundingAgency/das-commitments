using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities.Reservations;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.Reservations.Api.Client;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ReservationValidationService : IReservationValidationService
    {
        private readonly IReservationsApiClient _apiClient;
        private readonly IMapper<ReservationValidationRequest, ValidationReservationMessage> _requestMapper;
        private readonly IMapper<ValidationResult, ReservationValidationResult> _resultMapper;

        public ReservationValidationService(IReservationsApiClient apiClient,
            IMapper<ReservationValidationRequest, ValidationReservationMessage> requestMapper,
            IMapper<ValidationResult, ReservationValidationResult> resultMapper)
        {
            _apiClient = apiClient;
            _requestMapper = requestMapper;
            _resultMapper = resultMapper;
        }

        public async Task<ReservationValidationResult> Validate(ReservationValidationRequest request, CancellationToken cancellationToken)
        {
            var mappedRequest = _requestMapper.Map(request);
            var result = await _apiClient.ValidateReservation(mappedRequest, cancellationToken);
            return _resultMapper.Map(result);
        }
    }
}
