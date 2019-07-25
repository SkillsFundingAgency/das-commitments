using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.ValidateReservation
{
    public class ValidateReservationHandler : IAsyncRequestHandler<ValidateReservationRequest, ValidateReservationResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IReservationValidationService _reservationValidationService;

        public ValidateReservationHandler(
            IApprenticeshipRepository apprenticeshipRepository,
            IReservationValidationService reservationValidationService)
        {
            _apprenticeshipRepository = apprenticeshipRepository ?? throw new ArgumentNullException(nameof(apprenticeshipRepository));
            _reservationValidationService = reservationValidationService ?? throw new ArgumentNullException(nameof(reservationValidationService));
        }

        public async Task<ValidateReservationResponse> Handle(ValidateReservationRequest request)
        {
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(request.ApprenticeshipId);

            var validationServiceRequest = new ReservationValidationServiceRequest
            {
                AccountId = apprenticeship.EmployerAccountId,
                ApprenticeshipId = request.ApprenticeshipId,
                ReservationId = apprenticeship.ReservationId,
                CommitmentId = apprenticeship.CommitmentId,
                TrainingCode = request.CourseCode ?? apprenticeship.TrainingCode,
                StartDate = request.StartDate ?? apprenticeship.StartDate,
                ProviderId = apprenticeship.ProviderId
            };

            var validationResult = await _reservationValidationService.CheckReservation(validationServiceRequest);

            return new ValidateReservationResponse
            {
                ReservationValidationResult = validationResult
            };
        }
    }
}