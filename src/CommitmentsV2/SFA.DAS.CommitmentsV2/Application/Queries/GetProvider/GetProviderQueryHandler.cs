using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetProvider;

public class GetProviderQueryHandler(Lazy<ProviderCommitmentsDbContext> db) : IRequestHandler<GetProviderQuery, GetProviderQueryResult>
{
    public async Task<GetProviderQueryResult> Handle(GetProviderQuery request, CancellationToken cancellationToken)
    {
        var result = await db.Value.Providers
            .Where(p => p.UkPrn == request.ProviderId)
            .Select(p => new GetProviderQueryResult(p.UkPrn, p.Name))
            .SingleOrDefaultAsync(cancellationToken);
            
        return result;
    }
}