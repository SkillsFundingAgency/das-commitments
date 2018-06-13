using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Orchestrators.Mappers;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Application.Queries.GetStatistics;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public class StatisticsOrchestrator : IStatisticsOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly ICommitmentsLogger _logger;
        private readonly IStatisticsMapper _statisticsMapper;

        public StatisticsOrchestrator(IMediator mediator, ICommitmentsLogger logger, IStatisticsMapper statisticsMapper)
        {
            _mediator = mediator;
            _logger = logger;
            _statisticsMapper = statisticsMapper;
        }

        public async Task<ConsistencyStatistics> GetStatistics()
        {
            _logger.Trace("Getting Statistics for consistency checks");

            var response = await _mediator.SendAsync(new GetStatisticsRequest());

            return _statisticsMapper.MapFrom(response.Data);
        }
    }
}