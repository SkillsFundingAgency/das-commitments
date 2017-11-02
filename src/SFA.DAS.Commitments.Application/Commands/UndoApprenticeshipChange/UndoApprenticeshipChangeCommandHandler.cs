using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Exceptions;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange
{
    public class UndoApprenticeshipChangeCommandHandler : AsyncRequestHandler<UndoApprenticeshipChangeCommand>
    {
        private readonly AbstractValidator<UndoApprenticeshipChangeCommand> _validator;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMessagePublisher _messagePublisher;

        public UndoApprenticeshipChangeCommandHandler(AbstractValidator<UndoApprenticeshipChangeCommand> validator, IApprenticeshipUpdateRepository apprenticeshipUpdateRepository, IApprenticeshipRepository apprenticeshipRepository, IMessagePublisher messagePublisher)
        {
            _validator = validator;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(UndoApprenticeshipChangeCommand command)
        {
            var pendingUpdate =
                await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(command.ApprenticeshipId);
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);

            ValidateCommand(command, pendingUpdate, apprenticeship);

            await _apprenticeshipUpdateRepository.UndoApprenticeshipUpdate(pendingUpdate, command.UserId);
            await SendApprenticeshipUpdateCancelledEvent(apprenticeship);
        }

        private async Task SendApprenticeshipUpdateCancelledEvent(Apprenticeship apprenticeship)
        {
            await _messagePublisher.PublishAsync(new ApprenticeshipUpdateCancelled(apprenticeship.EmployerAccountId, apprenticeship.ProviderId, apprenticeship.Id));
        }

        private void ValidateCommand(UndoApprenticeshipChangeCommand command, ApprenticeshipUpdate pendingUpdate,
            Apprenticeship apprenticeship)
        {
            var result = _validator.Validate(command);
            if (!result.IsValid)
                throw new ValidationException("Did not validate");

            if (pendingUpdate == null)
                throw new ValidationException(
                    $"No existing apprenticeship update pending for apprenticeship {command.ApprenticeshipId}");

            CheckAuthorisation(command, apprenticeship);
        }

        private void CheckAuthorisation(UndoApprenticeshipChangeCommand command, Apprenticeship apprenticeship)
        {
            switch (command.Caller.CallerType)
            {
                case CallerType.Employer:
                    if (apprenticeship.EmployerAccountId != command.Caller.Id)
                        throw new UnauthorizedException(
                            $"Employer {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}");
                    break;
                case CallerType.Provider:
                    if (apprenticeship.ProviderId != command.Caller.Id)
                        throw new UnauthorizedException(
                            $"Provider {command.Caller.Id} not authorised to update apprenticeship {apprenticeship.Id}, expected provider {apprenticeship.ProviderId}");
                    break;
            }
        }
    }
}
