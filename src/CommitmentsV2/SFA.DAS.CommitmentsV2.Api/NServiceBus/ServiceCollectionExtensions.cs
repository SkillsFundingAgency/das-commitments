using System;
using System.Data.Common;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using NServiceBus.Settings;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;
using SFA.DAS.NServiceBus.Configuration.StructureMap;
using SFA.DAS.NServiceBus.Features.ClientOutbox.Data;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.Services;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using StructureMap;
using StructureMap.Pipeline;

namespace SFA.DAS.CommitmentsV2.Api.NServiceBus
{
    public static class ServiceCollectionExtensions
    {
        private const string EndpointName = "SFA.DAS.CommitmentsV2.Api";

        public static IServiceCollection AddNServiceBus(this IServiceCollection services)
        {
            services
                .AddSingleton(p =>
                {
                    var container = p.GetService<IContainer>();
                    var hostingEnvironment = p.GetService<IWebHostEnvironment>();
                    var configuration = p.GetService<CommitmentsV2Configuration>().NServiceBusConfiguration;
                    var runInDevelopmentMode = hostingEnvironment.IsDevelopment() || hostingEnvironment.EnvironmentName == Domain.Constants.IntegrationTestEnvironment;

                    var endpointConfiguration = new EndpointConfiguration(EndpointName)
                        .UseErrorQueue($"{EndpointName}-errors")
                        .UseInstallers()
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseStructureMapBuilder(container)
                        .UseUnitOfWork();

                    if (runInDevelopmentMode)
                    {
                        endpointConfiguration.UseLearningTransport(s => s.AddRouting());
                    }
                    else
                    {
                        endpointConfiguration.UseAzureServiceBusTransport(configuration.SharedServiceBusEndpointUrl, s => s.AddRouting());
                    }

                    if (!string.IsNullOrEmpty(configuration.NServiceBusLicense))
                    {
                        endpointConfiguration.UseLicense(configuration.NServiceBusLicense);
                    }
                    
                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                    // This is to replace the ClientOutboxPersisterV2 class in NSB with our local version
                    // as the local class is compatible with the EF concurrency issues
                    container.EjectAllInstancesOf<IClientOutboxStorageV2>();
                    container.Configure(config =>
                        {
                            config.For<IClientOutboxStorageV2>().Use<ClientOutboxPersisterV2>().LifecycleIs<UniquePerRequestLifecycle>();
                        }
                    );
                    
                    return endpoint;
                })
                .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();

            return services;
        }
    }
}