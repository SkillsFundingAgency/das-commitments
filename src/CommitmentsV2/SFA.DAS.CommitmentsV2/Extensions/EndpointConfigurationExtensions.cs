using NServiceBus;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Extensions;

[ExcludeFromCodeCoverage]
public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration ConfigureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder, bool isLocal, NServiceBusConfiguration nServiceBusConfiguration = null)
    {
        if (ShouldUseServiceBus(isLocal,nServiceBusConfiguration))
        {
            config.UseAzureServiceBusTransport(connectionStringBuilder(), s => s.AddRouting());
        }
        else
        {
            config.UseLearningTransport(learningTransportFolderPath: nServiceBusConfiguration.LearningTransportFolderPath);
        }

        config.UseMessageConventions();

        return config;
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

    private static bool ShouldUseServiceBus(bool isLocal, NServiceBusConfiguration configuration)
    {
        if (!isLocal)
            return true; // Always use service bus in non-development environments

        if (configuration.UseServiceBusInDev)
            return true; // Use service bus in development if configured to do so

        return false; // Otherwise, use learning transport

    }
}