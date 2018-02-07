using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.RejectDataLockTriage
{
    public class RejectDataLockTriageCommandHandler : AsyncRequestHandler<RejectDataLockTriageCommand>
    {
        private readonly AbstractValidator<RejectDataLockTriageCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMessagePublisher _messagePublisher;

        public RejectDataLockTriageCommandHandler(
            AbstractValidator<RejectDataLockTriageCommand> validator,
            IDataLockRepository dataLockRepository,
            IApprenticeshipRepository apprenticeshipRepository,
            IMessagePublisher messagePublisher)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(RejectDataLockTriageCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            var dataLocksToBeUpdated = await GetDataLocksToBeUpdated(command, apprenticeship);

            if (!dataLocksToBeUpdated.Any())
                return;
           
            await _dataLockRepository.UpdateDataLockTriageStatus(
                dataLocksToBeUpdated.Select(m => m.DataLockEventId),
                TriageStatus.Unknown);

            await PublishEvents(apprenticeship);
        }

        private async Task<IEnumerable<DataLockStatus>> GetDataLocksToBeUpdated(RejectDataLockTriageCommand command, Apprenticeship apprenticeship)
        {
            var datalocks = await _dataLockRepository.GetDataLocks(command.ApprenticeshipId);

            var dataLocksToBeUpdated = datalocks
                .Where(DataLockExtensions.UnHandled)
                .Where(x => x.TriageStatus == TriageStatus.Change);

            if (apprenticeship.HasHadDataLockSuccess)
            {
                dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockExtensions.IsPriceOnly);
            }
            return dataLocksToBeUpdated;
        }

        private async Task PublishEvents(Apprenticeship apprenticeship)
        {
            await _messagePublisher.PublishAsync(new DataLockTriageRejected(apprenticeship.EmployerAccountId, apprenticeship.ProviderId, apprenticeship.Id));
        }
    }
}