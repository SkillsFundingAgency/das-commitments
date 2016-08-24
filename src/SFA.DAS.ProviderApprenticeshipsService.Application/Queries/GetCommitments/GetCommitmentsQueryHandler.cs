using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using Newtonsoft.Json;
using SFA.DAS.ProviderApprenticeshipsService.Domain;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitments
{
    public class GetCommitmentsQueryHandler : IAsyncRequestHandler<GetCommitmentsQueryRequest, GetCommitmentsQueryResponse>
    {
        private readonly ProviderApprenticeshipsServiceConfiguration _configuration;

        public GetCommitmentsQueryHandler(ProviderApprenticeshipsServiceConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            _configuration = configuration;
        }

        public async Task<GetCommitmentsQueryResponse> Handle(GetCommitmentsQueryRequest message)
        {
            var content = "";
            var url = $"{_configuration.Api.BaseUrl}api/commitments/{message.ProviderId}";

            using (var client = new HttpClient())
            {
                var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);

                // Add custom headers
                //requestMessage.Headers.Add("User-Agent", "User-Agent-Here");

                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("WRAP", "bigAccessToken");
                var response = await client.SendAsync(requestMessage);
                content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }

            return new GetCommitmentsQueryResponse
            {
                Commitments = JsonConvert.DeserializeObject<List<CommitmentView>>(content)
            };
        }
    }
}