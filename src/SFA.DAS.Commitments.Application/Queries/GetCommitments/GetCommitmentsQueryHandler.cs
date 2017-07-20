using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitments
{
    public sealed class GetCommitmentsQueryHandler : IAsyncRequestHandler<GetCommitmentsRequest, GetCommitmentsResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetCommitmentsRequest> _validator;

        public GetCommitmentsQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetCommitmentsRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetCommitmentsResponse> Handle(GetCommitmentsRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitments = await GetCommitments(message.Caller);

            return new GetCommitmentsResponse
            {
                Data = commitments
            };
        }

        private async Task<IList<CommitmentSummary>> GetCommitments(Caller caller)
        {
            switch (caller.CallerType)
            {
                case CallerType.Employer:
                    return await _commitmentRepository.GetCommitmentsByEmployer(caller.Id);
                case CallerType.Provider:
                    return await _commitmentRepository.GetCommitmentsByProvider(caller.Id);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
