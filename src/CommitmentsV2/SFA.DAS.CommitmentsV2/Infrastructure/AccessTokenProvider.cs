using Microsoft.Azure.Services.AppAuthentication;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Infrastructure;

public class AccessTokenProvider(LevyTransferMatchingApiConfiguration configuration) : IAccessTokenProvider
{
    // Take advantage of built-in token caching
    private readonly AzureServiceTokenProvider _tokenProvider = new();
    
    public async Task<string> GetAccessToken()
    {
        return await _tokenProvider.GetAccessTokenAsync(configuration.Identifier);
    }
}