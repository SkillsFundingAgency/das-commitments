using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public class StatisticsMapper : IStatisticsMapper
    {
        public Statistics MapFrom(ConsistencyStatistics statistics)
        {
            var domainStatistics = new Statistics
            {
                TotalCohorts = statistics.TotalCohorts,
                TotalApprenticeships = statistics.TotalApprenticeships,
                ActiveApprenticeships = statistics.ActiveApprenticeships
            };

            return domainStatistics;
        }

        public ConsistencyStatistics MapFrom(Statistics statistics)
        {
            var stats = new ConsistencyStatistics
            {
                TotalCohorts = statistics.TotalCohorts,
                TotalApprenticeships = statistics.TotalApprenticeships,
                ActiveApprenticeships = statistics.ActiveApprenticeships
            };
            return stats;
        }
    }
}