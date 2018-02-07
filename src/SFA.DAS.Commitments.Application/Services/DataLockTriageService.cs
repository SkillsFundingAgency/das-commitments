using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Extensions;

namespace SFA.DAS.Commitments.Application.Services
{
    internal class DataLockTriageService
    {
        internal IEnumerable<DataLockStatus> GetDataLocksToBeUpdated(List<DataLockStatus> datalocksForApprenticeship, Apprenticeship apprenticeship)
        {
            var dataLocksToBeUpdated = datalocksForApprenticeship
                .Where(DataLockExtensions.UnHandled)
                .Where(m => m.TriageStatus == TriageStatus.Change);

            if (apprenticeship.HasHadDataLockSuccess)
            {
                dataLocksToBeUpdated = dataLocksToBeUpdated.Where(DataLockExtensions.IsPriceOnly);
            }
            return dataLocksToBeUpdated;
        }
    }
}
