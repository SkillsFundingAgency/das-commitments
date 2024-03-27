using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider
{
    public class GetProviderQueryHandler : IRequestHandler<GetProviderQuery, GetProviderQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _db;

        public GetProviderQueryHandler(Lazy<ProviderCommitmentsDbContext> db)
        {
            _db = db;
        }

        public async Task<GetProviderQueryResult> Handle(GetProviderQuery request, CancellationToken cancellationToken)
        {
            var result = await _db.Value.Providers
                .Where(p => p.UkPrn == request.ProviderId)
                .Select(p => new GetProviderQueryResult(p.UkPrn, p.Name))
                .SingleOrDefaultAsync(cancellationToken);
            
            return result;
        }
    }
}
