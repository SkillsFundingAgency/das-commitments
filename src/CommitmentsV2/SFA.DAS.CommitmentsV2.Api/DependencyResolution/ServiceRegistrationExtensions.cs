using Microsoft.Extensions.Configuration;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.LinkGeneration;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;
using SFA.DAS.CommitmentsV2.Shared.Services;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution;

public static class ServiceRegistrationExtensions
{
    public static IServiceCollection AddDefaultServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<IDbContextFactory, SynchronizedDbContextFactory>();
        services.AddSingleton<IAuthenticationService, AuthenticationService>();

        services.AddTransient<IModelMapper, ModelMapper>();
        services.AddSingleton<ILinkGenerator, LinkGenerator>();

        if (config["UseStubProviderRelationships"] != null && config["UseStubProviderRelationships"].Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
        {
            services.AddTransient<IProviderRelationshipsApiClient, StubProviderRelationshipsApiClient>();
        }

        return services;
    }
}
