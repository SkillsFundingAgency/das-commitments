using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IStatisticsRepository
    {
        Task<Statistics> GetStatistics();
    }
}
