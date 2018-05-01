using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Learners.Validators;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln
{
    public sealed class GetApprenticeshipsByUlnQueryHandler : IAsyncRequestHandler<GetApprenticeshipsByUlnRequest, GetApprenticeshipsByUlnResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly IUlnValidator _ulnValidator;

        public GetApprenticeshipsByUlnQueryHandler(IApprenticeshipRepository apprenticeshipRepository, IUlnValidator ulnValidator, ICommitmentsLogger logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _ulnValidator = ulnValidator;
            _logger = logger;
        }

        public async Task<GetApprenticeshipsByUlnResponse> Handle(GetApprenticeshipsByUlnRequest request)
        {
            ValidateRequest(request);

            var result = await _apprenticeshipRepository.GetApprenticeshipsByUln(request.Uln);

            return new GetApprenticeshipsByUlnResponse
            {
                Apprenticeships = result.Apprenticeships,
                TotalCount = result.TotalCount
            };
        }

        private void ValidateRequest(GetApprenticeshipsByUlnRequest request)
        {

            if (String.IsNullOrWhiteSpace(request.Uln))
            {
                throw new ValidationException(ValidationErrorMessage(UlnValidationResult.IsEmptyUlnNumber));
            }

            var validationResult = _ulnValidator.Validate(request.Uln);

            if (validationResult != UlnValidationResult.Success)
            {
                _logger.Warn($"Invalid Uln {request.Uln}");

                throw new ValidationException(ValidationErrorMessage(validationResult));
            }
        }

        private string ValidationErrorMessage(UlnValidationResult validationResult)
        {
            switch (validationResult)
            {
                case UlnValidationResult.IsEmptyUlnNumber:
                    return "Please enter a Unl Number";
                case UlnValidationResult.IsInValidTenDigitUlnNumber:
                    return "Please enter a Ten Digit Unl Number";
                default:
                    return "Please enter a Valid Unl";
            }
        }


    }
}