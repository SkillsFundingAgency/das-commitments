using System;
using NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;

namespace SFA.DAS.Commitments.Api.DependencyResolution
{
    public static class EndpointConfigurationExtensions
    {
        public static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder, bool isDevelopment)
        {
            config.UseAzureServiceBusTransport(isDevelopment, connectionStringBuilder, r => { });

            return config;
        }

        public static EndpointConfiguration UseDasMessageConventions(this EndpointConfiguration config)
        {
            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("SFA.DAS.CommitmentsV2.Messages.Commands"));

            conventions.DefiningEventsAs(t => t.Namespace != null &&
            (
            t.Namespace.StartsWith("SFA.DAS.CommitmentsV2.Messages.Events") ||
            t.Namespace.StartsWith("SFA.DAS.Commitments.Events"
            )));

            return config;
        }
    }
}