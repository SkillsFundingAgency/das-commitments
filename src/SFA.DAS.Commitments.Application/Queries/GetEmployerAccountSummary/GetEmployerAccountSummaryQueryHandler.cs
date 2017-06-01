using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary
{
    public sealed class GetEmployerAccountSummaryQueryHandler : IAsyncRequestHandler<GetEmployerAccountSummaryRequest, GetEmployerAccountSummaryResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly AbstractValidator<GetEmployerAccountSummaryRequest> _validator;

        public GetEmployerAccountSummaryQueryHandler(IApprenticeshipRepository apprenticeshipRepository, AbstractValidator<GetEmployerAccountSummaryRequest> validator)
        {
            _apprenticeshipRepository = apprenticeshipRepository;
            _validator = validator;
        }

        public async Task<GetEmployerAccountSummaryResponse> Handle(GetEmployerAccountSummaryRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeshipSummaries = await _apprenticeshipRepository.GetApprenticeshipSummariesByEmployer(message.Caller.Id);

            return new GetEmployerAccountSummaryResponse
            {
                Data = apprenticeshipSummaries
            };
        }
    }
}
