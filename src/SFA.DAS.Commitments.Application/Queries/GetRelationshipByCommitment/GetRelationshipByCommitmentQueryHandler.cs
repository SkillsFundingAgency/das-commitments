using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment
{
    public sealed class GetRelationshipByCommitmentQueryHandler :
        IAsyncRequestHandler<GetRelationshipByCommitmentRequest, GetRelationshipByCommitmentResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly AbstractValidator<GetRelationshipByCommitmentRequest> _validator;

        public GetRelationshipByCommitmentQueryHandler(ICommitmentRepository commitmentRepository,
            AbstractValidator<GetRelationshipByCommitmentRequest> validator,
            IRelationshipRepository relationshipRepository)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _relationshipRepository = relationshipRepository;
        }

        public async Task<GetRelationshipByCommitmentResponse> Handle(GetRelationshipByCommitmentRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(message.CommitmentId);

            CheckAuthorisation(message.Caller.Id, commitment);

            var entity = await _relationshipRepository.GetRelationship(commitment.EmployerAccountId,
                commitment.ProviderId.Value, commitment.LegalEntityId);

            if (entity == null)
            {
                return new GetRelationshipByCommitmentResponse();
            }

            return new GetRelationshipByCommitmentResponse
            {
                Data = entity
            };
        }

        private static void CheckAuthorisation(long providerId, Commitment commitment)
        {
            if (providerId != commitment.ProviderId.Value)
            {
                throw new UnauthorizedException($"Provider {providerId} not authorised to access commitment {commitment.Id}");
            }
        }
    }
}