using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Commitments.Support.SubSite.Configuration;

namespace SFA.DAS.Commitments.Support.SubSite.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDasDistributedMemoryCache(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
        {
            var redisConnectionString = configuration.GetValue<string>(CommitmentsSupportConfigurationKeys.RedisConnectionString);

            if (isDevelopment)
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                services.AddStackExchangeRedisCache(o => o.Configuration = redisConnectionString);
            }

            return services;
        }
    }
}