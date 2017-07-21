using System.Threading.Tasks;
using FluentValidation;
using MediatR;

using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Application.Rules;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitment
{
    public sealed class GetCommitmentQueryHandler : IAsyncRequestHandler<GetCommitmentRequest, GetCommitmentResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetCommitmentRequest> _validator;

        private readonly ICommitmentRules _commitmentRules;

        public GetCommitmentQueryHandler(
            ICommitmentRepository commitmentRepository, 
            AbstractValidator<GetCommitmentRequest> validator,
            ICommitmentRules commitmentRules)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _commitmentRules = commitmentRules;
        }

        public async Task<GetCommitmentResponse> Handle(GetCommitmentRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(message.CommitmentId);

            if (commitment == null)
            {
                return new GetCommitmentResponse { Data = null };
            }

            CheckAuthorization(message, commitment);

            return new GetCommitmentResponse
            {
                Data = commitment
            };
        }


        private static void CheckAuthorization(GetCommitmentRequest message, Domain.Entities.Commitment commitment)
        {
            switch (message.Caller.CallerType)
            {
                case CallerType.Provider:
                    if (commitment.ProviderId != message.Caller.Id)
                        throw new UnauthorizedException($"Provider {message.Caller.Id} not authorised to access commitment {message.CommitmentId}, expected provider {commitment.ProviderId}");
                    break;
                case CallerType.Employer:
                default:
                    if (commitment.EmployerAccountId != message.Caller.Id)
                        throw new UnauthorizedException($"Employer {message.Caller.Id} not authorised to access commitment {message.CommitmentId}, expected employer {commitment.EmployerAccountId}");
                    break;
            }
        }
    }
}
