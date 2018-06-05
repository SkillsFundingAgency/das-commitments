using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetStatistics
{
    public class GetStatisticsQueryHandler : IAsyncRequestHandler<GetStatisticsRequest, GetStatisticsResponse>
    {
        private readonly IStatisticsRepository _statisticsRepository;

        public GetStatisticsQueryHandler(IStatisticsRepository statisticsRepository)
        {
            _statisticsRepository = statisticsRepository;
        }

        public async Task<GetStatisticsResponse> Handle(GetStatisticsRequest message)
        {
            var stats = await _statisticsRepository.GetStatistics();  //.ConfigureAwait(false)

            if (stats == null)
            {
                return new GetStatisticsResponse {Data = null};
            }

            return new GetStatisticsResponse {Data = stats};
        }
    }
}