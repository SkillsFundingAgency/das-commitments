using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationship
{
    public sealed class GetRelationshipQueryHandler : IAsyncRequestHandler<GetRelationshipRequest, GetRelationshipResponse>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<GetRelationshipRequest> _validator;

        public GetRelationshipQueryHandler(ICommitmentRepository commitmentRepository, AbstractValidator<GetRelationshipRequest> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<GetRelationshipResponse> Handle(GetRelationshipRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var entity = await _commitmentRepository.GetRelationship(message.EmployerAccountId, message.ProviderId,
                message.LegalEntityId);

            if (entity == null)
            {
                return new GetRelationshipResponse();
            }

            var result = new GetRelationshipResponse { Data = entity };

            return result;
        }

    }
}
