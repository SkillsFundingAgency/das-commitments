using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.NServiceBus;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.NServiceBus.StructureMap;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs.NServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNServiceBus(this IServiceCollection services)
        {
            return services
                .AddSingleton(s =>
                {
                    var container = s.GetService<IContainer>();
                    var hostingEnvironment = s.GetService<IHostingEnvironment>();
                    var configuration = s.GetService<CommitmentsV2Configuration>().NServiceBusConfiguration;
                    var isDevelopment = hostingEnvironment.IsDevelopment();

                    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.CommitmentsV2.Jobs")
                        .UseAzureServiceBusTransport(() => configuration.ServiceBusConnectionString, isDevelopment)
                        .UseErrorQueue()
                        .UseInstallers()
                        .UseLicense(configuration.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseStructureMapBuilder(container)
                        .UseSendOnly();

                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                    return endpoint;
                })
                .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}