using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.Commitments.Application.Services
{
    public class ReservationValidationService : IReservationValidationService
    {
        private readonly IReservationsApiClient _reservationsApiClient;
        private readonly ICommitmentsLogger _logger;
        private static readonly ReservationValidationResult NoValidationRequiredResponse =
            new ReservationValidationResult(new ReservationValidationError[0]);

        public ReservationValidationService(
            IReservationsApiClient reservationsApiClient,
            ICommitmentsLogger logger)
        {
            _reservationsApiClient = reservationsApiClient ?? throw new ArgumentNullException(nameof(reservationsApiClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<ReservationValidationResult> CheckReservation(ReservationValidationServiceRequest request)
        {
            return CheckReservationWithLogging(request);
        }

        private async Task<ReservationValidationResult> CheckReservationWithLogging(ReservationValidationServiceRequest request)
        {
            try
            {
                return await CheckReservationIfRequired(request);
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Failed to check reservation",
                    request.AccountId,
                    request.ProviderId,
                    request.CommitmentId,
                    request.ApprenticeshipId);
                throw;
            }
        }

        private async Task<ReservationValidationResult> CheckReservationIfRequired(ReservationValidationServiceRequest request)
        {
            if (request.ReservationId == null)
            {
                _logger.Info($"Commitment:{request.CommitmentId} Apprenticeship: {request.ApprenticeshipId} Reservation-id:null - no reservation validation required");
                return NoValidationRequiredResponse;
            }

            if (request.StartDate == null)
            {
                throw new ValidationException(
                    $"Unable to validate the reservation because the start date is absent");
            }

            var validationReservationMessage = new ReservationValidationMessage
            {
                ReservationId = request.ReservationId.Value,
                CourseCode = request.TrainingCode,
                StartDate = request.StartDate.Value
            };

            var validationResult =
                await _reservationsApiClient.ValidateReservation(validationReservationMessage, CancellationToken.None);

            return validationResult;
        }
    }
}