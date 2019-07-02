﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client
{
    public class CommitmentsApiClientFactory : ICommitmentsApiClientFactory
    {
        private readonly CommitmentsClientApiConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory;

        public CommitmentsApiClientFactory(CommitmentsClientApiConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _loggerFactory = loggerFactory;
        }
        
        public ICommitmentsApiClient CreateClient()
        {
            var httpClientFactory = new AzureActiveDirectoryHttpClientFactory(_configuration, _loggerFactory);
            var httpClient = httpClientFactory.CreateHttpClient();
            var restHttpClient = new CommitmentsRestHttpClient(httpClient);
            var apiClient = new CommitmentsApiClient(restHttpClient);

            return apiClient;
        }
    }
}
