using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllProviders
{
    public class GetAllProvidersQueryHandler : IRequestHandler<GetAllProvidersQuery, GetAllProvidersQueryResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        public GetAllProvidersQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetAllProvidersQueryResult> Handle(GetAllProvidersQuery request, CancellationToken cancellationToken)
        {
            var providers = _dbContext.Value.Providers.ToList();

            var providerDetails = new List<Provider>();

            foreach (var provider in providers)
            {
                providerDetails.Add(new Provider { Ukprn = provider.UkPrn, Name = provider.Name });
            }

            return Task.FromResult(new GetAllProvidersQueryResult(providerDetails));
        }
    }
}