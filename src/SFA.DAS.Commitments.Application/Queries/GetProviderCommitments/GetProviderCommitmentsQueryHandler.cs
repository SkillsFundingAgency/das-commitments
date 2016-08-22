using System.Threading.Tasks;
using FluentValidation;
using MediatR;
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
            if (_validator.Validate(message).IsValid)
            {
                var commitments = await _commitmentRepository.GetByProvider(message.ProviderId);

                return new GetProviderCommitmentsResponse { Commitments = commitments };
            }

            return new GetProviderCommitmentsResponse { HasError = true };
        }
    }
}
