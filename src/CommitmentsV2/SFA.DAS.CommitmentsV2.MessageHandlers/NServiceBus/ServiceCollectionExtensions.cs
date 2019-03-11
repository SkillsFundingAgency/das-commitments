using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;
using SFA.DAS.NServiceBus.SqlServer;
using SFA.DAS.NServiceBus.StructureMap;
using SFA.DAS.UnitOfWork.NServiceBus;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.NServiceBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureNServiceBus(this IServiceCollection services)
        {
            return services
                .AddSingleton<IEndpointInstance>(s =>
                {
                    var container = s.GetService<IContainer>();
                    var hostingEnvironment = s.GetService<IHostingEnvironment>();
                    var configuration = s.GetService<CommitmentsV2Configuration>().NServiceBusConfiguration;
                    var isDevelopment = hostingEnvironment.IsDevelopment();

                    var endpointConfiguration = new EndpointConfiguration("SFA.DAS.CommitmentsV2.MessageHandlers")
                        .UseAzureServiceBusTransport(() => configuration.ServiceBusConnectionString, isDevelopment)
                        .UseInstallers()
                        .UseLicense(configuration.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseStructureMapBuilder(container)
                        .UseUnitOfWork();

                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                    return endpoint;
                });
        }
    }
}