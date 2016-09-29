using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using CommitmentStatus = SFA.DAS.Commitments.Api.Types.CommitmentStatus;

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

            return new GetCommitmentsResponse { Data = commitments?.Select(
                    x => new CommitmentListItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ProviderId = x.ProviderId,
                        ProviderName = x.ProviderName,
                        EmployerAccountId = x.EmployerAccountId,
                        EmployerAccountName = "",
                        LegalEntityId = x.LegalEntityId,
                        LegalEntityName = x.LegalEntityName,
                        Status = (CommitmentStatus)x.Status
                    }
                ).ToList()
            };
        }

        private async Task<IList<Domain.Commitment>> GetCommitments(Caller caller)
        {
            switch (caller.CallerType)
            {
                case CallerType.Provider:
                    return await _commitmentRepository.GetByProvider(caller.Id);
                case CallerType.Employer:
                default:
                    return await _commitmentRepository.GetByEmployer(caller.Id);
            }
        }
    }
}
