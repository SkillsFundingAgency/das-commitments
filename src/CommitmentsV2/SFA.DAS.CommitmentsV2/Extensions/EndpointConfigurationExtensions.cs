using NServiceBus;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;

namespace SFA.DAS.CommitmentsV2.Extensions;

[ExcludeFromCodeCoverage]
public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration ConfigureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder, bool isLocal, string learningTransportFolderPath = null)
    {
        if (isLocal)
        {
            config.UseLearningTransport(learningTransportFolderPath: learningTransportFolderPath);
        }
        else
        {
            config.UseAzureServiceBusTransport(connectionStringBuilder(), s => s.AddRouting());
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
}