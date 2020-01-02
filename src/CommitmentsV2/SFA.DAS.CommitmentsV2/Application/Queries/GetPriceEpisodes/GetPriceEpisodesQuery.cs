using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes
{
    public class GetPriceEpisodesQuery: IRequest<GetPriceEpisodesQueryResult>
    {
        public long ApprenticeshipId { get; private set; }

        public GetPriceEpisodesQuery(long apprenticeshipId)
        {
            ApprenticeshipId = apprenticeshipId;
        }
    }
}
