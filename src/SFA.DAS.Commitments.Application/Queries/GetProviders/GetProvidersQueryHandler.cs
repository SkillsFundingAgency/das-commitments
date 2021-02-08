using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetProviders
{
    public class GetProvidersQueryHandler : IAsyncRequestHandler<GetProvidersQuery, GetProvidersQueryResponse>
    {
        private readonly IProviderRepository _providerRepository;

        public GetProvidersQueryHandler (IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }
        
        public async Task<GetProvidersQueryResponse> Handle(GetProvidersQuery message)
        {
            var providers = await _providerRepository.GetProviders();
            
            return new GetProvidersQueryResponse
            {
                Providers = providers.Select(c=> new ProviderResponse
                {
                    Name = c.Name,
                    Ukprn = c.Ukprn
                }).ToList()
            }; 
                
        }
    }
}