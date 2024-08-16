using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Http.MessageHandlers;
using SFA.DAS.Http.TokenGenerators;

namespace SFA.DAS.ProviderRelationships.Api.Client
{
    public static class AddProviderRelationshipsApiClientExtension
    {
        public static IServiceCollection AddProviderRelationshipsApiClient(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("ProviderRelationshipsApi").Get<ProviderRelationshipsApiConfiguration>();
            services
                .AddHttpClient<IProviderRelationshipsApiClient, ProviderRelationshipsApiClient>(options =>
                {
                    options.BaseAddress = new Uri(config.ApiBaseUrl);
                })
                .AddHttpMessageHandler(() => new VersionHeaderHandler())
                .AddHttpMessageHandler(() => new ManagedIdentityHeadersHandler(new ManagedIdentityTokenGenerator(config)));
            return services;
        }
    }
}
