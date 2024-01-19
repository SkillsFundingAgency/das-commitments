using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Api.Authentication;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.LinkGeneration;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.ProviderRelationships.Api.Client;
using SFA.DAS.ProviderRelationships.Api.Client.Http;
using StructureMap;
using StructureMap.Building.Interception;
using System;

namespace SFA.DAS.CommitmentsV2.Api.DependencyResolution
{
    public static class ServiceRegistrationExtensions 
    {


        public static IServiceCollection AddDefaultServices(this IServiceCollection services, IConfiguration config)
        {

            services.AddTransient<IDbContextFactory, SynchronizedDbContextFactory>();
            services.AddSingleton<IAuthenticationService, AuthenticationService>();

            services.AddTransient<IModelMapper, ModelMapper>();
            services.AddSingleton<ILinkGenerator, LinkGenerator>();

            if (config["UseStubProviderRelationships"].Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
            {
                // TODO check this overrides a previously registered implementation
                services.AddTransient<IProviderRelationshipsApiClient, StubProviderRelationshipsApiClient>();
                services.AddTransient<IProviderRelationshipsApiClientFactory, StubProviderRelationshipsApiClientFactory>();
            }
            else
            {
                // TODO really not sure this is actually needed
                services.AddTransient<IProviderRelationshipsApiClient, ProviderRelationshipsApiClient>();
                services.AddTransient<IProviderRelationshipsApiClientFactory, ProviderRelationshipsApiClientFactory>();
            }

            return services;
        }


    }
}