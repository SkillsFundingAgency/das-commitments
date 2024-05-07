using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.Payments.ProviderPayments.Messages;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        private const string EndpointName = "SFA.DAS.CommitmentsV2.ExternalHandlers";

        public static IServiceCollection AddNServiceBus(this IServiceCollection services)
        {
            return services
                .AddSingleton(p =>
                {
                    var hostingEnvironment = p.GetService<IHostEnvironment>();
                    var configuration = p.GetService<CommitmentsV2Configuration>();
                    var isDevelopment = hostingEnvironment.IsDevelopment();

                    var endpointConfiguration = new EndpointConfiguration(EndpointName)
                        .UseLicense(configuration.NServiceBusConfiguration.NServiceBusLicense)
                        .UseErrorQueue($"{EndpointName}-errors")
                        .UseInstallers()
                        .UseMessageConventions()
                        .UseNewtonsoftJsonSerializer()
                        .UseNLogFactory()
                        .UseOutbox()
                        .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(configuration.DatabaseConnectionString))
                        .UseUnitOfWork()
                        .UseServicesBuilder(new UpdateableServiceProvider(services));

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
                        endpointConfiguration.UseAzureServiceBusTransport(configuration.NServiceBusConfiguration.SharedServiceBusEndpointUrl,s => s.AddRouting());
                    }
                    
                    var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

                    return endpoint;
                })
                .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
                .AddHostedService<NServiceBusHostedService>();
        }
    }
}