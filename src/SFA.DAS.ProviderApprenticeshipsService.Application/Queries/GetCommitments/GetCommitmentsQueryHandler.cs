using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using MediatR;
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
            var url = "";

            using (var client = new HttpClient())
            {
                //var body = $"client_secret={_configuration.Api.ClientSecret}&redirect_uri={redirectUrl}&code={code}";
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("WRAP", "bigAccessToken");
                var response = await client.GetAsync(""); //, new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                content = await response.Content.ReadAsStringAsync();
                response.EnsureSuccessStatusCode();
            }

            return new GetCommitmentsQueryResponse
            {
                Commitments = null
            };
        }
    }
}