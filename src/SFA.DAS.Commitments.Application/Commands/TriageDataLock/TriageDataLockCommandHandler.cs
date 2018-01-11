using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Events;
using SFA.DAS.Messaging.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.TriageDataLock
{
    public sealed class TriageDataLockCommandHandler : AsyncRequestHandler<TriageDataLockCommand>
    {
        private readonly AbstractValidator<TriageDataLockCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository; 

        private readonly ICommitmentsLogger _logger;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public TriageDataLockCommandHandler(
            AbstractValidator<TriageDataLockCommand> validator,
            IDataLockRepository dataLockRepository, 
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            ICommitmentsLogger logger,
            IMessagePublisher messagePublisher,
            IApprenticeshipRepository apprenticeshipRepository)
        {
            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
            _messagePublisher = messagePublisher;
            _apprenticeshipRepository = apprenticeshipRepository;
        }

        protected override async Task HandleCore(TriageDataLockCommand message)
        {
            var validationResult = _validator.Validate(message);

            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }

            var dataLock = await _dataLockRepository.GetDataLock(message.DataLockEventId);

            AssertDataLockBelongsToApprenticeship(message.ApprenticeshipId, dataLock);

            var triageStatus = (TriageStatus)message.TriageStatus;

            if (dataLock.TriageStatus == triageStatus)
            {
                _logger.Warn($"Trying to update data lock for apprenticeship: {message.ApprenticeshipId} with the same TriageStatus ({message.TriageStatus}) ");
                return;
            }

            AssertValidTriageStatus(triageStatus, dataLock);
            await AssertNoPendingApprenticeshipUpdate(dataLock, message.ApprenticeshipId);

            if (triageStatus == TriageStatus.Change)
            {
                await CreateEventIfThereWereNoExistingDataLocksRequiringApprovalForTheApprenticeship(message);
            }

            await _dataLockRepository.UpdateDataLockTriageStatus(message.DataLockEventId, triageStatus);
        }

        private async Task CreateEventIfThereWereNoExistingDataLocksRequiringApprovalForTheApprenticeship(TriageDataLockCommand message)
        {
            var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(message.ApprenticeshipId);
            var dataLocksRequiringApproval = await GetExistingDataLocksRequiringApproval(message, apprenticeship);
            if (!dataLocksRequiringApproval.Any())
            {
                await _messagePublisher.PublishAsync(new DataLockTriageRequiresApproval(apprenticeship.EmployerAccountId, apprenticeship.ProviderId, apprenticeship.Id));
            }
        }

        private async Task<IEnumerable<DataLockStatus>> GetExistingDataLocksRequiringApproval(TriageDataLockCommand message, Apprenticeship apprenticeship)
        {
            var existingDataLocks = await _dataLockRepository.GetDataLocks(message.ApprenticeshipId);
            var dataLockService = new DataLockTriageService();
            var dataLocksRequiringApproval = dataLockService.GetDataLocksToBeUpdated(existingDataLocks, apprenticeship);
            return dataLocksRequiringApproval;
        }

        private void AssertDataLockBelongsToApprenticeship(long apprenticeshipId, DataLockStatus dataLockStatus)
        {
            if (apprenticeshipId != dataLockStatus.ApprenticeshipId)
            {
                throw new ValidationException($"Data lock {dataLockStatus.DataLockEventId} does not belong to Apprenticeship {apprenticeshipId}");
            }
        }

        private void AssertValidTriageStatus(TriageStatus triageStatus, DataLockStatus dataLockStatus)
        {
            if (triageStatus == TriageStatus.Change)
            {
                if (!(dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07)
                    || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock09)))
                {
                    throw new ValidationException($"Data lock {dataLockStatus.DataLockEventId} with error code {dataLockStatus.ErrorCode} cannot be triaged as {triageStatus}");
                }
            }

            if (triageStatus == TriageStatus.Restart)
            {
                if (!(dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                      || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                      || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                      || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06)
                      ))
                {
                    throw new ValidationException($"Data lock {dataLockStatus.DataLockEventId} with error code {dataLockStatus.ErrorCode} cannot be triaged as {triageStatus}");
                }
            }
        }

        private async Task AssertNoPendingApprenticeshipUpdate(DataLockStatus dataLockStatus, long apprenticeshipId)
        {
            var pending = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(apprenticeshipId);
            if(pending != null)
            {
                throw new ValidationException($"Data lock {dataLockStatus.DataLockEventId} with error code {dataLockStatus.ErrorCode} cannot be triaged due to apprenticeship {apprenticeshipId} having pending update");
            }
        }
    }
}
