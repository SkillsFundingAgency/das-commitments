using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using System.Linq;

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
                Data = MapFrom(apprenticeshipSummaries)
            };
        }

        private IEnumerable<Api.Types.ApprenticeshipStatusSummary> MapFrom(IEnumerable<Domain.Entities.ApprenticeshipStatusSummary> apprenticeshipSummaries)
        {
            return apprenticeshipSummaries.Select(s => new Api.Types.ApprenticeshipStatusSummary
            {
                LegalEntityIdentifier = s.LegalEntityIdentifier,
                PendingApprovalCount = s.PendingApprovalCount,
                ActiveCount = s.ActiveCount,
                PausedCount = s.PausedCount,
                WithdrawnCount = s.WithdrawnCount,
                CompletedCount = s.CompletedCount
            });
        }
    }
}
