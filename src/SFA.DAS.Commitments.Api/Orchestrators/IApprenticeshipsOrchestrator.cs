using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types.DataLock;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public interface IApprenticeshipsOrchestrator
    {
        Task<Types.DataLock.DataLockStatus> GetDataLock(long apprenticeshipId, long dataLockEventId);
        Task<IEnumerable<Types.DataLock.DataLockStatus>> GetDataLocks(long apprenticeshipId, Caller caller);
        Task<DataLockSummary> GetDataLockSummary(long apprenticeshipId, Caller caller);
        Task TriageDataLock(long apprenticeshipId, long dataLockEventId, DataLockTriageSubmission triageSubmission, Caller caller);
        Task TriageDataLocks(long apprenticeshipId, DataLockTriageSubmission triageSubmission, Caller caller);
        Task ResolveDataLock(long apprenticeshipId, DataLocksTriageResolutionSubmission triageSubmission, Caller caller);
        Task<IEnumerable<Types.Apprenticeship.PriceHistory>> GetPriceHistory(long apprenticeshipId, Caller caller);
    }
}