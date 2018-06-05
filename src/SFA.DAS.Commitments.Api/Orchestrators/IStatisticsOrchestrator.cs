using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public interface IStatisticsOrchestrator
    {
        Task<ConsistencyStatistics> GetStatistics();
    }
}