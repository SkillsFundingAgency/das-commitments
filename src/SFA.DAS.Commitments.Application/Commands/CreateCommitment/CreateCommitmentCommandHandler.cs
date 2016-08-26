using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.CreateCommitment
{
    public sealed class CreateCommitmentCommandHandler : AsyncRequestHandler<CreateCommitmentCommand>
    {
        private CreateCommitmentValidator _createCommitmentValidator;
        private ICommitmentRepository _commitmentRepository;

        public CreateCommitmentCommandHandler(ICommitmentRepository commitmentRepository, CreateCommitmentValidator createCommitmentValidator)
        {
            _commitmentRepository = commitmentRepository;
            _createCommitmentValidator = createCommitmentValidator;
        }

        protected override async Task HandleCore(CreateCommitmentCommand message)
        {
            if (!_createCommitmentValidator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            await _commitmentRepository.Create(MapFrom(message.Commitment));
        }

        private Domain.Commitment MapFrom(Commitment commitment)
        {
            throw new NotImplementedException();
        }
    }
}
