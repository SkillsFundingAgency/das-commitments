using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;
using SFA.DAS.NServiceBus.Configuration.StructureMap;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.Payments.ProviderPayments.Messages;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.NServiceBus
{
    public static class ServiceCollectionExtensions
    {
        private const string EndpointName = "SFA.DAS.CommitmentsV2.ExternalHandlers";

        public static IServiceCollection AddNServiceBus(this IServiceCollection services)
        {
            return services
                .AddSingleton(p =>
                {
                    var container = p.GetService<IContainer>();
                    var hostingEnvironment = p.GetService<IWebHostEnvironment>();
                    var configuration = p.GetService<CommitmentsV2Configuration>().NServiceBusConfiguration;
                    var isDevelopment = hostingEnvironment.IsDevelopment();

                    var endpointConfiguration = new EndpointConfiguration(EndpointName)
                        .UseErrorQueue($"{EndpointName}-errors")
                        .UseInstallers()
                        .UseLicense(configuration.NServiceBusLicense)
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => container.GetInstance<DbConnection>())
                        .UseStructureMapBuilder(container)
                        .UseUnitOfWork();

                    endpointConfiguration.Conventions().DefiningEventsAs(t =>
                        t == typeof(RecordedAct1CompletionPayment) ||
                        t == typeof(EntityStateChangedEvent) ||
                        t.Name.EndsWith("Event"));

                    if (isDevelopment)
                    {
                        endpointConfiguration.UseLearningTransport(s => s.AddRouting());
                    }
                    else
                    {
                        endpointConfiguration.UseAzureServiceBusTransport(configuration.SharedServiceBusEndpointUrl,s => s.AddRouting());
                    }
                    
                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                    return endpoint;
                })
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}