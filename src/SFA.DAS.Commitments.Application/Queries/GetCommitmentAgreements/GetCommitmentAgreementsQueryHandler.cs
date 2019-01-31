using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitmentAgreements
{
    public sealed class GetCommitmentAgreementsQueryHandler : IAsyncRequestHandler<GetCommitmentAgreementsRequest, GetCommitmentAgreementsResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetCommitmentAgreementsRequest> _validator;

        public GetCommitmentAgreementsQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetCommitmentAgreementsRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetCommitmentAgreementsResponse> Handle(GetCommitmentAgreementsRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitmentAgreements = await GetCommitmentAgreements(message.Caller);

            return new GetCommitmentAgreementsResponse
            {
                Data = commitmentAgreements
            };
        }

        private async Task<IList<CommitmentAgreement>> GetCommitmentAgreements(Caller caller)
        {
            switch (caller.CallerType)
            {
                case CallerType.Provider:
                    return await _commitmentRepository.GetCommitmentAgreementsForProvider(caller.Id);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
