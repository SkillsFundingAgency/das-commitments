using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Caching;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDasDistributedMemoryCache(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var redisConnectionString = configuration.GetValue<string>(CommitmentsConfigurationKeys.RedisConnectionString);
            
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