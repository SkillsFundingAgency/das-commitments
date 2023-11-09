using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes
{
    public class GetPriceEpisodesQueryHandler : IRequestHandler<GetPriceEpisodesQuery, GetPriceEpisodesQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetPriceEpisodesQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<GetPriceEpisodesQueryResult> Handle(GetPriceEpisodesQuery request, CancellationToken cancellationToken)
        {
            return new GetPriceEpisodesQueryResult
            {
                PriceEpisodes = await _dbContext.Value.PriceHistory
                    .Where(x => x.ApprenticeshipId == request.ApprenticeshipId)
                    .Select(priceHistory => new GetPriceEpisodesQueryResult.PriceEpisode
                    {
                        Id = priceHistory.Id,
                        ApprenticeshipId = priceHistory.ApprenticeshipId,
                        FromDate = priceHistory.FromDate,
                        ToDate = priceHistory.ToDate,
                        Cost = priceHistory.Cost
                    }).ToListAsync(cancellationToken)
            };
        }
    }
}
