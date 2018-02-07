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

namespace SFA.DAS.Commitments.Application.Commands.TriageDataLocks
{
    public sealed class TriageDataLocksCommandHandler : AsyncRequestHandler<TriageDataLocksCommand>
    {
        private readonly AbstractValidator<TriageDataLocksCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IMessagePublisher _messagePublisher;

        public TriageDataLocksCommandHandler(AbstractValidator<TriageDataLocksCommand> validator, IDataLockRepository dataLockRepository, IApprenticeshipRepository apprenticeshipRepository, IMessagePublisher messagePublisher)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _messagePublisher = messagePublisher;
        }

        protected override async Task HandleCore(TriageDataLocksCommand command)
        {
            var validationResult = _validator.Validate(command);
            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.Errors);

            var dataLocksToBeUpdated = await GetDataLocksToBeUpdated(command);

            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(command.ApprenticeshipId);
            Validate(command, dataLocksToBeUpdated, apprenticeship);

            if (apprenticeship.HasHadDataLockSuccess && command.TriageStatus == TriageStatus.Change)
            {
                dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockExtensions.IsPriceOnly).ToList();
            }

            if (dataLocksToBeUpdated.Any(m => m.TriageStatus == command.TriageStatus))
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with the same TriageStatus ({command.TriageStatus}) ");
            }

            await SendApprovalMessageWhenStatusIsChange(command, apprenticeship);

            await _dataLockRepository.UpdateDataLockTriageStatus(dataLocksToBeUpdated.Select(m => m.DataLockEventId), command.TriageStatus);
        }

        private async Task SendApprovalMessageWhenStatusIsChange(TriageDataLocksCommand command, Apprenticeship apprenticeship)
        {
            if (command.TriageStatus == TriageStatus.Change)
            {
                await _messagePublisher.PublishAsync(new DataLockTriageRequiresApproval(apprenticeship.EmployerAccountId, apprenticeship.ProviderId, apprenticeship.Id));
            }
        }

        private async Task<List<DataLockStatus>> GetDataLocksToBeUpdated(TriageDataLocksCommand command)
        {
            var dataLocksToBeUpdated = (await _dataLockRepository
                    .GetDataLocks(command.ApprenticeshipId))
                .Where(DataLockExtensions.UnHandled)
                .ToList();
            return dataLocksToBeUpdated;
        }

        private static void Validate(TriageDataLocksCommand command, List<DataLockStatus> dataLocksToBeUpdated, Apprenticeship apprenticeship)
        {
            var courseAndPriceOrOnlyCourse = dataLocksToBeUpdated.All(DataLockExtensions.WithCourseError)
                                          || dataLocksToBeUpdated.Any(DataLockExtensions.WithCourseAndPriceError);
            if (   courseAndPriceOrOnlyCourse
                && apprenticeship.HasHadDataLockSuccess
                && command.TriageStatus == TriageStatus.Change)
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with triage status ({command.TriageStatus}) and datalock with course and price when Successful DataLock already received");
            }
        }
    }
}
