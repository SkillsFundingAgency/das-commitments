using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentValidation;

using SFA.DAS.Commitments.Application.Commands.UpdateDataLocksTriageStatus;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Commands
{
    public class DataLockValidation : IDataLockValidation
    {
        private readonly IDataLockRepository _dataLockRepository;

        private readonly IApprenticeshipUpdateRepository _apprenticeshipUpdateRepository;

        public DataLockValidation(
            IDataLockRepository dataLockRepository, 
            IApprenticeshipUpdateRepository apprenticeshipUpdateRepository)
        {
            if (dataLockRepository == null)
                throw new ArgumentNullException(nameof(IDataLockRepository));
            if (apprenticeshipUpdateRepository == null)
                throw new ArgumentNullException(nameof(IApprenticeshipUpdateRepository));

            _dataLockRepository = dataLockRepository;
            _apprenticeshipUpdateRepository = apprenticeshipUpdateRepository;
        }

        public async Task Assert(UpdateDataLocksTriageStatusCommand command, IEnumerable<DataLockStatus> dataLocksToBeUpdated)
        {
            var triageStatus = (TriageStatus)command.TriageStatus;

            // Assure no DataLocks with the same status as before
            if (dataLocksToBeUpdated.Any(m => m.TriageStatus == triageStatus))
            {
                throw new ValidationException($"Trying to update data lock for apprenticeship: {command.ApprenticeshipId} with the same TriageStatus ({command.TriageStatus}) ");
            }

            // Ensure Apprenticeship do not have any updates pending
            await AssertNoPendingApprenticeshipUpdate(dataLocksToBeUpdated.ToListString(), command.ApprenticeshipId);
        }

        private void AssertValidTriageStatus(TriageStatus triageStatus, DataLockStatus dataLockStatus)
        {
            if (triageStatus == TriageStatus.Change)
            {
                if (!dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07))
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

        private async Task AssertNoPendingApprenticeshipUpdate(string dataLockStatus, long apprenticeshipId)
        {
            var pending = await _apprenticeshipUpdateRepository.GetPendingApprenticeshipUpdate(apprenticeshipId);
            if (pending != null)
            {
                throw new ValidationException($"Data lock(s) {dataLockStatus}  with error code cannot be triaged due to apprenticeship {apprenticeshipId} having pending update");
            }
        }
    }

    public interface IDataLockValidation
    {
        Task Assert(UpdateDataLocksTriageStatusCommand command, IEnumerable<DataLockStatus> dataLocksToBeUpdated);
    }
}
