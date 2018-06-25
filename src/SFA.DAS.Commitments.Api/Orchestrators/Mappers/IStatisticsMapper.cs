using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface IStatisticsMapper
    {
        Statistics MapFrom(ConsistencyStatistics statistics);
        ConsistencyStatistics MapFrom(Statistics statistics);
    }
}