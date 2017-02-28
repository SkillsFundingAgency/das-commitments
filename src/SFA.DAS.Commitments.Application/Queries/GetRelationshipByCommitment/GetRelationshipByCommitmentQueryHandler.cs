using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Application.Queries.GetRelationship;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using Relationship = SFA.DAS.Commitments.Api.Types.Relationship;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationshipByCommitment
{
    public sealed class GetRelationshipByCommitmentQueryHandler :
        IAsyncRequestHandler<GetRelationshipByCommitmentRequest, GetRelationshipByCommitmentResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetRelationshipByCommitmentRequest> _validator;

        public GetRelationshipByCommitmentQueryHandler(ICommitmentRepository commitmentRepository,
            AbstractValidator<GetRelationshipByCommitmentRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetRelationshipByCommitmentResponse> Handle(GetRelationshipByCommitmentRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var commitment = await _commitmentRepository.GetCommitmentById(message.CommitmentId);

            CheckAuthorisation(message.ProviderId, commitment);

            var entity = await _commitmentRepository.GetRelationship(commitment.EmployerAccountId,
                commitment.ProviderId.Value, commitment.LegalEntityId);

            return new GetRelationshipByCommitmentResponse
            {
                Data = new Relationship
                {
                    EmployerAccountId = entity.EmployerAccountId,
                    Id = entity.Id,
                    LegalEntityId = entity.LegalEntityId,
                    LegalEntityName = entity.LegalEntityName,
                    ProviderId = entity.ProviderId,
                    ProviderName = entity.ProviderName,
                    Verified = entity.Verified,
                }
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