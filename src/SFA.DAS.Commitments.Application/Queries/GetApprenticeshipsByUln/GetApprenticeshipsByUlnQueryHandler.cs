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
            try
            {
                ValidateRequest(request);

                var result = await _apprenticeshipRepository.GetApprenticeshipsByUln(request.Uln, request.EmployerAccountId);

                return new GetApprenticeshipsByUlnResponse
                {
                    Apprenticeships = result.Apprenticeships,
                    TotalCount = result.TotalCount
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to get Apprentiships for ULN");
                throw ex;
            }
        }

        private void ValidateRequest(GetApprenticeshipsByUlnRequest request)
        {
            var validationMsg = $"Invalid Uln {request.Uln}";

            if (String.IsNullOrWhiteSpace(request.Uln))
            {
                throw new ValidationException(validationMsg);
            }

            var validationResult = _ulnValidator.Validate(request.Uln);

            if (validationResult != UlnValidationResult.Success)
            {
                _logger.Warn(validationMsg);

                throw new ValidationException(validationMsg);
            }
        }


    }
}