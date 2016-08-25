using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitment
{
    public sealed class GetCommitmentQueryHandler : IAsyncRequestHandler<GetCommitmentRequest, GetCommitmentResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetCommitmentRequest> _validator;

        public GetCommitmentQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetCommitmentRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetCommitmentResponse> Handle(GetCommitmentRequest message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                return new GetCommitmentResponse { HasErrors = true };
            }

            var commitment = await _commitmentRepository.GetById(message.CommitmentId);

            return new GetCommitmentResponse { Data = commitment };
        }
    }
}
