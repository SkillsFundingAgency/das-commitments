using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class ApprenticeshipOrchestrator
    {
        public ApprenticeshipOrchestrator()
        {
            
        }

        public async Task GetDataLock(long apprenticeshipId, long dataLockEventId)
        {
            throw new System.NotImplementedException();
        }

        public async Task GetDataLocks(long apprenticeshipId)
        {
            throw new System.NotImplementedException();
        }

        public async Task PatchDataLock(long apprenticeshipId, DataLockStatus datalock)
        {
            throw new System.NotImplementedException();
        }
    }
}