using System;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Application.Commands.UpdateDataLockTriageStatus
{
    public sealed class UpdateDataLockTriageStatusCommandHandler : AsyncRequestHandler<UpdateDataLockTriageStatusCommand>
    {
        private readonly AbstractValidator<UpdateDataLockTriageStatusCommand> _validator;
        private readonly IDataLockRepository _dataLockRepository;

        public UpdateDataLockTriageStatusCommandHandler(AbstractValidator<UpdateDataLockTriageStatusCommand> validator,
            IDataLockRepository dataLockRepository)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(AbstractValidator<UpdateDataLockTriageStatusCommand>));
            if(dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));

            _validator = validator;
            _dataLockRepository = dataLockRepository;
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
                return;
            }

            ApprenticeshipUpdate apprenticeshipUpdate = null;
            if (triageStatus == TriageStatus.Change)
            {
                apprenticeshipUpdate = new ApprenticeshipUpdate
                {
                    ApprenticeshipId = dataLock.ApprenticeshipId,
                    Originator = Originator.Provider,
                    UpdateOrigin = UpdateOrigin.DataLock
                    //todo: update origin
                };

                if ((dataLock.ErrorCode & DataLockErrorCode.Dlock07) == DataLockErrorCode.Dlock07)
                {
                    apprenticeshipUpdate.Cost = dataLock.IlrTotalCost;
                }

                if ((dataLock.ErrorCode & DataLockErrorCode.Dlock09) == DataLockErrorCode.Dlock09)
                {
                    apprenticeshipUpdate.StartDate = dataLock.IlrActualStartDate;
                    //todo: this field? or go get the apprenticeship and use the start date as per Mark's notes?
                }

                //todo: ChangeEffectiveDates? Not clear.

            }

            await _dataLockRepository.UpdateDataLockTriageStatus(message.DataLockEventId, triageStatus, apprenticeshipUpdate);
        }

        private void AssertDataLockBelongsToApprenticeship(long apprenticeshipId, DataLockStatus dataLockStatus)
        {
            if (apprenticeshipId != dataLockStatus.ApprenticeshipId)
            {
                throw new ValidationException($"Data lock {dataLockStatus.DataLockEventId} does not belong to Apprenticeship {apprenticeshipId}");
            }
        }
    }
}
