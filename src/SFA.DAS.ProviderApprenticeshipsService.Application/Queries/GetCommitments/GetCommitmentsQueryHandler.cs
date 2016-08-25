using System;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Commitments.Api.Client;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitments
{
    public class GetCommitmentsQueryHandler : IAsyncRequestHandler<GetCommitmentsQueryRequest, GetCommitmentsQueryResponse>
    {
        private readonly ProviderApprenticeshipsServiceConfiguration _configuration;
        private readonly ICommitmentsApi _commitmentsApi;

        public GetCommitmentsQueryHandler(ProviderApprenticeshipsServiceConfiguration configuration, ICommitmentsApi commitmentsApi)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (commitmentsApi == null)
                throw new ArgumentNullException(nameof(commitmentsApi));
            _configuration = configuration;
            _commitmentsApi = commitmentsApi;
        }

        public async Task<GetCommitmentsQueryResponse> Handle(GetCommitmentsQueryRequest message)
        {
            var response = await _commitmentsApi.GetForProvider(message.ProviderId);

            return new GetCommitmentsQueryResponse
            {
                Commitments = response
            };
        }
    }
}