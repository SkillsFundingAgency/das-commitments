using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus
{
    public sealed class UpdateDataLockTriageStatusCommandHandler : AsyncRequestHandler<UpdateDataLockTriageStatusCommand>
    {
        private readonly AbstractValidator<UpdateDataLockTriageStatusCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;
        private readonly IApprenticeshipRepository _apprenticeshipRepository;
        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository; 

        private readonly ICommitmentsLogger _logger;

        public UpdateDataLockTriageStatusCommandHandler(
            AbstractValidator<UpdateDataLockTriageStatusCommand> validator,
            IDataLockRepository dataLockRepository, 
            IApprenticeshipRepository apprenticeshipRepository,
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository,
            ICommitmentsLogger logger)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<UpdateDataLockTriageStatusCommand>));
            if(dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if(apprenticeshipRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipRepository));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if(apprenticeshipUpdateRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipUpdateRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
            _apprenticeshipRepository = apprenticeshipRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
            _logger = logger;
        }

        protected override async Task HandleCore(UpdateDataLockTriageStatusCommand message)
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

            ApprenticeshipUpdate apprenticeshipUpdate = null;
            if (triageStatus == TriageStatus.Change)
            {
                var apprenticeship = await _apprenticeshipRepository.GetApprenticeship(message.ApprenticeshipId);

                apprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ApprenticeshipId = dataLock.ApprenticeshipId,
                    Originator = Originator.Provider,
                    UpdateOrigin = UpdateOrigin.DataLock,
                    EffectiveFromDate = dataLock.IlrEffectiveFromDate.Value,
                    EffectiveToDate = null
                };

                if ((dataLock.ErrorCode & DataLockErrorCode.Dlock07) == DataLockErrorCode.Dlock07)
                {
                    apprenticeshipUpdate.Cost = dataLock.IlrTotalCost;
                }

                if ((dataLock.ErrorCode & DataLockErrorCode.Dlock09) == DataLockErrorCode.Dlock09)
                {
                    apprenticeshipUpdate.StartDate = dataLock.IlrActualStartDate;
                }
            }

            var isResolved = triageStatus != TriageStatus.Unknown;

            await _dataLockRepository.UpdateDataLockTriageStatus(message.DataLockEventId, triageStatus, apprenticeshipUpdate, isResolved);
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
