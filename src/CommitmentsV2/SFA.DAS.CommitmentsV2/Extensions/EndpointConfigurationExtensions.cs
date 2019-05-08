using System;
using NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class EndpointConfigurationExtensions
    {
        public static EndpointConfiguration UseAzureServiceBusTransport(this EndpointConfiguration config, Func<string> connectionStringBuilder, bool isDevelopment)
        {
            config.UseAzureServiceBusTransport(isDevelopment, connectionStringBuilder, r => { });

            return config;
        }
    }
}
