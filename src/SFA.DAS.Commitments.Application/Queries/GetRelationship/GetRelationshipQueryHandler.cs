using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetRelationship
{
    public sealed class GetRelationshipQueryHandler : IAsyncRequestHandler<GetRelationshipRequest, GetRelationshipResponse>
    {
        private readonly IRelationshipRepository _relationshipRepository;
        private readonly AbstractValidator<GetRelationshipRequest> _validator;

        public GetRelationshipQueryHandler(IRelationshipRepository relationshipRepository, AbstractValidator<GetRelationshipRequest> validator)
        {
            _relationshipRepository = relationshipRepository;
            _validator = validator;
        }

        public async Task<GetRelationshipResponse> Handle(GetRelationshipRequest message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var entity = await _relationshipRepository.GetRelationship(message.EmployerAccountId, message.ProviderId,
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
