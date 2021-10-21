using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure
{
    public class AccessTokenProvider : IAccessTokenProvider
    {
        private readonly LevyTransferMatchingApiConfiguration _configuration;

        public AccessTokenProvider(LevyTransferMatchingApiConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GetAccessToken()
        {
            var tokenProvider = new AzureServiceTokenProvider();
            var getTokenTask = tokenProvider.GetAccessTokenAsync(_configuration.Identifier);
            return getTokenTask.GetAwaiter().GetResult();
        }
    }
}
