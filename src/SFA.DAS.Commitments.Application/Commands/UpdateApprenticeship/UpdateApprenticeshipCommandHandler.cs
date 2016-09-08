using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeship
{
    public sealed class UpdateApprenticeshipCommandHandler : AsyncRequestHandler<UpdateApprenticeshipCommand>
    {
        private readonly ICommitmentRepository _commitmentRepository;
        private readonly AbstractValidator<UpdateApprenticeshipCommand> _validator;

        public UpdateApprenticeshipCommandHandler(ICommitmentRepository commitmentRepository, AbstractValidator<UpdateApprenticeshipCommand> validator)
        {
            _commitmentRepository = commitmentRepository;
            _validator = validator;
        }

        protected override async Task HandleCore(UpdateApprenticeshipCommand message)
        {
            if (!_validator.Validate(message).IsValid)
            {
                throw new InvalidRequestException();
            }

            await _commitmentRepository.UpdateApprenticeship(MapFrom(message.Apprenticeship, message));
        }

        private Domain.Apprenticeship MapFrom(Api.Types.Apprenticeship apprenticeship, UpdateApprenticeshipCommand message)
        {
            var domainApprenticeship = new Domain.Apprenticeship
            {
                Id = message.ApprenticeshipId,
                FirstName = apprenticeship.FirstName,
                LastName = apprenticeship.LastName,
                ULN = apprenticeship.ULN,
                CommitmentId = message.CommitmentId,
                Status = (Domain.ApprenticeshipStatus)apprenticeship.Status,
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
