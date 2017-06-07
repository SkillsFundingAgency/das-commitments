using MediatR;

namespace SFA.DAS.Commitments.Application.Queries.GetPriceEpisodes
{
    public sealed class GetPriceEpisodesRequest : IAsyncRequest<GetPriceEpisodesResponse>
    {
        public long ApprenticeshipId { get; set; }
    }
}