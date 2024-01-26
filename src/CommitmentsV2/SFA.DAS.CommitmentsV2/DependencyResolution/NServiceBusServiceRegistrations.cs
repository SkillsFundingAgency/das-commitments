using NServiceBus.ObjectBuilder.MSDependencyInjection;
using NServiceBus;
using SFA.DAS.NServiceBus.Hosting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.CommitmentsV2.Configuration;
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
    public static void StartNServiceBus(this UpdateableServiceProvider services, bool isDevOrLocal)
    {
        var endPointName = "SFA.DAS.CommitmentsV2.API";
        var commitmentsConfiguration = services.GetService<CommitmentsV2Configuration>();

        var databaseConnectionString = commitmentsConfiguration.DatabaseConnectionString;

        if (string.IsNullOrEmpty(databaseConnectionString))
        {
            throw new Exception("DatabaseConnectionString");
        }

        var endpointConfiguration = new EndpointConfiguration(endPointName)
            .UseErrorQueue($"{endPointName}-errors")
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