using NServiceBus;
using System.Diagnostics.CodeAnalysis;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;

namespace SFA.DAS.CommitmentsV2.Extensions;

[ExcludeFromCodeCoverage]
public static class EndpointConfigurationExtensions
{
    public static EndpointConfiguration ConfigureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder, bool isLocal)
    {
        if (isLocal)
        {
            config.UseLearningTransport();
        }
        else
        {
            config.UseAzureServiceBusTransport(connectionStringBuilder(), s => s.AddRouting());
        }

        config.UseMessageConventions();

        return config;
    }
}