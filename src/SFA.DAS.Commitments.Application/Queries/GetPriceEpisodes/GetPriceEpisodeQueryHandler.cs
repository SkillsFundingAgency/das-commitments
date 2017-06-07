using System;
using System.Threading.Tasks;

using MediatR;

using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetPriceEpisodes
{
    public class GetPriceEpisodeQueryHandler : IAsyncRequestHandler<GetPriceEpisodesRequest, GetPriceEpisodesResponse>
    {
        private readonly IApprenticeshipRepository _apprenticeshipRepository;

        public GetPriceEpisodeQueryHandler(IApprenticeshipRepository apprenticeshipRepository)
        {
            if(apprenticeshipRepository == null)
                throw new ArgumentNullException($"{nameof(IApprenticeshipUpdateRepository)} cannot be null");

            _apprenticeshipRepository = apprenticeshipRepository;
        }

        public async Task<GetPriceEpisodesResponse> Handle(GetPriceEpisodesRequest message)
        {
            return new 
                GetPriceEpisodesResponse
                {
                    Data = await _apprenticeshipRepository.GetPriceEpisodes(message.ApprenticeshipId)
                };
        }
    }
}