using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange
{
    public class RejectApprenticeshipChangeCommandHandler : AsyncRequestHandler<RejectApprenticeshipChangeCommand>
    {
        private readonly AbstractValidator<RejectApprenticeshipChangeCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public RejectApprenticeshipChangeCommandHandler(AbstractValidator<RejectApprenticeshipChangeCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, IApprenticeshipRepository apprenticeshipRepository)
        {
            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        protected override async Task HandleCore(RejectApprenticeshipChangeCommand command)
        {
            var pendingUpdate = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(command.ApprenticeshipId);
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            ValidateCommand(command, pendingUpdate, apprenticeship);

            await _apprenticeshipUpdateRepository.RejectApprenticeshipUpdate(pendingUpdate, command.UserId);
        }

        private void ValidateCommand(RejectApprenticeshipChangeCommand command, ApprenticeshipUpdate pendingUpdate, Apprenticeship apprenticeship)
        {
            var result = _validator.Validate(command);
            if (!result.IsValid)
                throw new ValidationException("Did not validate");

            if (pendingUpdate == null)
                throw new ValidationException($"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");

            CheckAuthorisation(command, apprenticeship);
        }
        
        private void CheckAuthorisation(RejectApprenticeshipChangeCommand command, Apprenticeship apprenticeship)
        {
            switch (command.Caller.CallerType)
            {
                case CallerType.Employer:
                    if (apprenticeship.EmployerAccountId != command.Caller.Id)
                        throw new UnauthorizedException($"Employer {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != command.Caller.Id)
                        throw new UnauthorizedException($"Provider {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}, expected provider {apprenticeship.ProviderId}");
                    break;
            }
        }
    }
}
