using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;
using Polly;
using Polly.Retry;

using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Commitments.Infrastructure.Configuration;
using SFA.DAS.Commitments.Infrastructure.Models;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class IdamsEmailServiceWrapper : IProviderEmailServiceWrapper
    {
        private readonly ICommitmentsLogger _logger;

        private readonly IHttpClientWrapper _httpClientWrapper;

        private readonly ProviderUserApiConfiguration _config;

        private readonly RetryPolicy _retryPolicy;

        public IdamsEmailServiceWrapper(
            ICommitmentsLogger logger,
            CommitmentsApiConfiguration configuration,
            IHttpClientWrapper httpClientWrapper)
        {
            _logger = logger;
            _config = configuration.ProviderUserApiConfiguration;
            _httpClientWrapper = httpClientWrapper;
            _retryPolicy = GetRetryPolicy();
        }

        public virtual async Task<List<ProviderUser>> GetUsersAsync(long ukprn)
        {
            var url = string.Format(_config.IdamsListUsersUrl, _config.DasUserRoleId, ukprn);
            _logger.Info($"Getting 'DAS' emails for provider {ukprn}");
            var result = await GetString(url, _config.ClientToken);
            return ParseIdamsResult(result, ukprn);
        }

        private List<ProviderUser> ParseIdamsResult(string jsonResult, long ukprn)
        {
            try
            {
                var result = JObject.Parse(jsonResult).SelectToken("result");

                if (result.Type == JTokenType.Array)
                {
                    var items = result.ToObject<IEnumerable<UserResponse>>();
                    return items.SelectMany(MapToProviderUser).ToList();
                }

                var item = result.ToObject<UserResponse>();
                return MapToProviderUser(item)
                    .ToList();
            }
            catch (Exception exception)
            {
                _logger.Info($"Result: {jsonResult}");
                _logger.Error(
                    exception,
                    $"Not possible to parse result to {typeof(UserResponse)} for provider: {ukprn}");
            }

            return new List<ProviderUser>();
        }

        private IEnumerable<ProviderUser> MapToProviderUser(UserResponse arg)
        {
            var providerUsers = new List<ProviderUser>();
            for (int i = 0; i < arg.Emails.Count; i++)
            {
                providerUsers.Add(
                    new ProviderUser
                    {
                        GivenName = arg.GivenNames[i],
                        FamilyName = arg.FamilyNames[i],
                        Email = arg.Emails[i],
                        Title = arg.Titles[i]
                    });
                
            }
            return providerUsers;
        }

        private async Task<string> GetString(string url, string accessToken)
        {
            var result = string.Empty;
            try
            {
                await _retryPolicy.ExecuteAsync(
                    async () =>
                    {
                        _httpClientWrapper.AuthScheme = "Bearer";
                        result = await _httpClientWrapper.GetString(url, accessToken);
                    });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting idams emails");
            }
            return result;
        }

        private Polly.Retry.RetryPolicy GetRetryPolicy()
        {
            return Policy
                    .Handle<Exception>()
                    .RetryAsync(3,
                        (exception, retryCount) =>
                        {
                            _logger.Warn($"Error connecting to Account Api: ({exception.Message}). Retrying...attempt {retryCount})");
                        }
                    );
        }

    }
}
