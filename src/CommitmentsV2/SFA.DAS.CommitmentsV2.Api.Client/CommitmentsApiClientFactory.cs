using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.Http;
using System;

namespace SFA.DAS.CommitmentsV2.Api.Client;

public class CommitmentsApiClientFactory(CommitmentsClientApiConfiguration configuration, ILoggerFactory loggerFactory)
    : ICommitmentsApiClientFactory
{
    public ICommitmentsApiClient CreateClient()
    {
        var httpClientFactory = new ManagedIdentityHttpClientFactory(configuration, loggerFactory);
        var httpClient = httpClientFactory.CreateHttpClient();
        var restHttpClient = new CommitmentsRestHttpClient(httpClient, loggerFactory);

        return new CommitmentsApiClient(restHttpClient);
    }
}