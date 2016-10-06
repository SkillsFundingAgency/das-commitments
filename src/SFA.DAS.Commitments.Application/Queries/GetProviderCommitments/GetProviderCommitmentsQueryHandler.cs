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

            var activeCommitments = commitments?.Where(x => x.Status == Domain.CommitmentStatus.Active);

            return new GetProviderCommitmentsResponse { Data = activeCommitments?.Select(
                    x => new CommitmentListItem
                    {
                        Id = x.Id,
                        Name = x.Name,
                        ProviderId = x.ProviderId,
                        ProviderName = x.ProviderName,
                        EmployerAccountId = x.EmployerAccountId,
                        EmployerAccountName = "",
                        LegalEntityCode = x.LegalEntityCode,
                        LegalEntityName = x.LegalEntityName,
                        Status = (CommitmentStatus)x.Status
                    }
                ).ToList() };
        }
    }
}
