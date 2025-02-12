using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Hosting;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.Extensions;

public static class ServiceCollectionExtensions
{
    private const string EndpointName = "SFA.DAS.CommitmentsV2.MessageHandlers";

    public static IServiceCollection AddNServiceBus(this IServiceCollection services)
    {
        return services
            .AddSingleton(serviceProvider =>
            {
                var hostingEnvironment = serviceProvider.GetService<IHostEnvironment>();
                var configuration = serviceProvider.GetService<CommitmentsV2Configuration>();
                var isDevelopment = hostingEnvironment.IsDevelopment();
             
                var endpointConfiguration = new EndpointConfiguration(EndpointName)
                    .UseLicense(configuration.NServiceBusConfiguration.NServiceBusLicense)
                    .UseErrorQueue($"{EndpointName}-errors")
                    .UseInstallers()
                    .UseMessageConventions()
                    .UseNewtonsoftJsonSerializer()
                    .UseOutbox()
                    .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(configuration.DatabaseConnectionString))
                    .UseUnitOfWork()
                    .UseServicesBuilder(new UpdateableServiceProvider(services));

                if (hostingEnvironment.ShouldUseServiceBus(configuration))
                {
                    endpointConfiguration.UseAzureServiceBusTransport(configuration.NServiceBusConfiguration.SharedServiceBusEndpointUrl, s => s.AddRouting()); 
                }
                else
                {
                    endpointConfiguration.UseLearningTransport(learningTransportFolderPath: configuration.NServiceBusConfiguration.LearningTransportFolderPath);
                }
                
                return Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }

    public static EndpointConfiguration UseLearningTransport(this EndpointConfiguration config, Action<RoutingSettings> routing = null, string learningTransportFolderPath = null)
    {
        TransportExtensions<LearningTransport> transportExtensions = config.UseTransport<LearningTransport>();
        if (!string.IsNullOrWhiteSpace(learningTransportFolderPath))
        {
            transportExtensions.StorageDirectory(learningTransportFolderPath);
        }

        transportExtensions.Transactions(TransportTransactionMode.ReceiveOnly);
        routing?.Invoke(transportExtensions.Routing());
        return config;
    }

    private static bool ShouldUseServiceBus(this IHostEnvironment hostingEnvironment, CommitmentsV2Configuration configuration)
    {
        var isDevelopment = hostingEnvironment.IsDevelopment();

        if (!isDevelopment)
            return true; // Always use service bus in non-development environments

        if(configuration.NServiceBusConfiguration.UseServiceBusInDev)
            return true; // Use service bus in development if configured to do so

        return false; // Otherwise, use learning transport

    }
}