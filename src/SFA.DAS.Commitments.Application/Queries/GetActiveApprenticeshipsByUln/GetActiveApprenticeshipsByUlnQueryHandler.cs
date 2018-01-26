using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Rules;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Queries.GetActiveApprenticeshipsByUln
{
    public sealed class GetActiveApprenticeshipsByUlnQueryHandler : IAsyncRequestHandler<GetActiveApprenticeshipsByUlnRequest, GetActiveApprenticeshipsByUlnResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly ICommitmentsLogger _logger;
        private readonly AbstractValidator<GetActiveApprenticeshipsByUlnRequest> _validator;

        public GetActiveApprenticeshipsByUlnQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetActiveApprenticeshipsByUlnRequest> validator, IApprenticeshipOverlapRules overlapRules, ICommitmentsLogger logger)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<GetActiveApprenticeshipsByUlnResponse> Handle(GetActiveApprenticeshipsByUlnRequest query)
        {
            var validationResult = _validator.Validate(query);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var result = new GetActiveApprenticeshipsByUlnResponse
            {
                Data = new List<ApprenticeshipResult>()
            };
            
            if (string.IsNullOrWhiteSpace(query.Uln))
                return result;

            var apprenticeships = await _apprenticeshipRepository.GetActiveApprenticeshipsByUlns(new List<string>(){query.Uln});

            foreach (var apprenticeship in apprenticeships)
            {
                _logger.Info($"Found active apprenticeships for ULN: {query.Uln} " +
                             $"Apprenticeship Id: {apprenticeship.Id} {apprenticeship.StartDate:MMM yyyy} - {apprenticeship.EndDate:MMM yyyy}");
                result.Data.Add(apprenticeship);
            }

            return result;
        }
    }
}
