using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NServiceBus.ObjectBuilder.MSDependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.Configuration.NLog;
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
                    .UseMessageConventions()
                    .UseInstallers()
                    .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(configuration.DatabaseConnectionString))
                    .UseNewtonsoftJsonSerializer()
                    .UseOutbox()
                    .UseNLogFactory()
                    .UseUnitOfWork()
                    .UseServicesBuilder(new UpdateableServiceProvider(services));

                if (isDevelopment)
                {
                    endpointConfiguration.UseLearningTransport(s => s.AddRouting());
                }
                else
                {
                    endpointConfiguration.UseAzureServiceBusTransport(configuration.NServiceBusConfiguration.SharedServiceBusEndpointUrl, s => s.AddRouting());
                }
                
                return Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            })
            .AddSingleton<IMessageSession>(s => s.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }
}