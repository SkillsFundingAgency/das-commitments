using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.VerifyRelationship
{
    public sealed class VerifyRelationshipCommandHandler: AsyncRequestHandler<VerifyRelationshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly VerifyRelationshipValidator _validator;
        private readonly ICommitmentsLogger _logger;

        public VerifyRelationshipCommandHandler(ICommitmentRepository commitmentRepository, VerifyRelationshipValidator validator, ICommitmentsLogger logger)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
            _logger = logger;
        }

        protected override async Task HandleCore(VerifyRelationshipCommand message)
        {
            _logger.Info($"Provider {message.ProviderId} has called VerifyRelationshipCommand for Employer {message.EmployerAccountId}, LegalEntity {message.LegalEntityId}");

            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            await _commitmentRepository.VerifyRelationship(message.EmployerAccountId, message.ProviderId, message.LegalEntityId);
        }
    }
}
