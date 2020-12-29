using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain.Data;

namespace SFA.DAS.Commitments.Application.Queries.GetProvider
{
    public class GetProviderQueryHandler : IAsyncRequestHandler<GetProviderQuery, GetProviderQueryResponse>
    {
        private readonly IProviderRepository _providerRepository;

        public GetProviderQueryHandler (IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }
        public async Task<GetProviderQueryResponse> Handle(GetProviderQuery message)
        {
            var result = await _providerRepository.GetProvider(message.Ukprn);
            
            return new GetProviderQueryResponse
            {
                Provider = new ProviderResponse
                {
                    Name = result.Name,
                    Ukprn = result.Ukprn
                } 
            };
        }
    }
}