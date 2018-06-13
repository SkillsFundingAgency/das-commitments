using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Client.Interfaces
{
    public interface IStatisticsApi
    {
        Task<ConsistencyStatistics> GetStatistics();
    }
}
