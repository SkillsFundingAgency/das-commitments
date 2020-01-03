using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;

namespace SFA.DAS.CommitmentsV2.Mapping.ResponseMappers
{
    public class GetPriceEpisodesResponseMapper : Shared.Interfaces.IMapper<GetPriceEpisodesQueryResult, GetPriceEpisodesResponse>
    {
        public Task<GetPriceEpisodesResponse> Map(GetPriceEpisodesQueryResult source)
        {
            return Task.FromResult(new GetPriceEpisodesResponse
            {
                PriceEpisodes = source.PriceEpisodes.Select(episode => new GetPriceEpisodesResponse.PriceEpisode
                    {
                        Id = episode.Id,
                        ApprenticeshipId = episode.ApprenticeshipId,
                        Cost = episode.Cost,
                        FromDate = episode.FromDate,
                        ToDate = episode.ToDate
                    }).ToList()
            });
        }
    }
}
