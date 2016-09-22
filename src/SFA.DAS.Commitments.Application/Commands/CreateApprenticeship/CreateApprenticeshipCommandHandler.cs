using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeship
{
    public sealed class CreateApprenticeshipCommandHandler : IAsyncRequestHandler<CreateApprenticeshipCommand, long>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<CreateApprenticeshipCommand> _validator;

        public CreateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<CreateApprenticeshipCommand> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        public async Task<long> Handle(CreateApprenticeshipCommand message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            var apprenticeshipId = await _commitmentRepository.CreateApprenticeship(MapFrom(message.Apprenticeship, message));

            return apprenticeshipId;
        }

        private Domain.Apprenticeship MapFrom(Apprenticeship apprenticeship, CreateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Domain.Apprenticeship
            {
                Id = apprenticeship.Id,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                Status = Domain.ApprenticeshipStatus.ReadyForApproval,
                AgreementStatus = (Domain.AgreementStatus)apprenticeship.AgreementStatus,
                TrainingId = apprenticeship.TrainingId,
                Cost = apprenticeship.Cost,
                StartDate = apprenticeship.StartDate,
                EndDate = apprenticeship.EndDate
            };

            return domainApprenticeship;
        }
    }
}
