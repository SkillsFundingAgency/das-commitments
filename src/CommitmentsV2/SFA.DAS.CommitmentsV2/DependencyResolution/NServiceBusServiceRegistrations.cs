using NServiceBus.ObjectBuilder.MSDependencyInjection;
using NServiceBus;
using SFA.DAS.NServiceBus.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.SqlServer.Configuration;
using SFA.DAS.UnitOfWork.NServiceBus.Configuration;

namespace SFA.DAS.CommitmentsV2.DependencyResolution;

[ExcludeFromCodeCoverage]
public static class NServiceBusServiceRegistrations
{
    private const string EndPointName = "SFA.DAS.CommitmentsV2.API";
    
    public static void StartNServiceBus(this UpdateableServiceProvider services, bool isDevOrLocal)
    {
        var commitmentsConfiguration = services.GetService<CommitmentsV2Configuration>();

        var databaseConnectionString = commitmentsConfiguration.DatabaseConnectionString;

        if (string.IsNullOrEmpty(databaseConnectionString))
        {
            throw new NullConnectionStringException("The DatabaseConnectionString provided is null or empty.");
        }

        var endpointConfiguration = new EndpointConfiguration(EndPointName)
            .UseErrorQueue($"{EndPointName}-errors")
            .UseInstallers()
            .UseMessageConventions()
            .UseServicesBuilder(services)
            .UseNewtonsoftJsonSerializer()
            .UseOutbox(true)
            .UseSqlServerPersistence(() => DatabaseExtensions.GetSqlConnection(databaseConnectionString))
            .ConfigureServiceBusTransport(() => commitmentsConfiguration.NServiceBusConfiguration.ServiceBusConnectionString, isDevOrLocal)
            .UseUnitOfWork();

        if (!string.IsNullOrEmpty(commitmentsConfiguration.NServiceBusConfiguration.NServiceBusLicense))
        {
            var decodedLicence = WebUtility.HtmlDecode(commitmentsConfiguration.NServiceBusConfiguration.NServiceBusLicense);
            endpointConfiguration.License(decodedLicence);
        }

        var endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();

        services.AddSingleton(p => endpoint)
            .AddSingleton<IMessageSession>(p => p.GetService<IEndpointInstance>())
            .AddHostedService<NServiceBusHostedService>();
    }
}

