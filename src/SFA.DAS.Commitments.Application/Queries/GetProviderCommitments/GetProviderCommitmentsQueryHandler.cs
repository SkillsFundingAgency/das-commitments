using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetProviderCommitments
{
    public sealed class GetProviderCommitmentsQueryHandler : IAsyncRequestHandler<GetProviderCommitmentsRequest, GetProviderCommitmentsResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetProviderCommitmentsRequest> _validator;

        public GetProviderCommitmentsQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetProviderCommitmentsRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetProviderCommitmentsResponse> Handle(GetProviderCommitmentsRequest message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            var commitments = await _commitmentRepository.GetByProvider(message.ProviderId);

            return new GetProviderCommitmentsResponse { Data = commitments?.Select(
                    x => new CommitmentListItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ProviderId = x.ProviderId,
                        ProviderName = x.ProviderName,
                        EmployerAccountId = x.EmployerAccountId,
                        EmployerAccountName = "",
                        LegalEntityId = x.LegalEntityId,
                        LegalEntityName = x.LegalEntityName
                    }
                ).ToList() };
        }
    }
}
