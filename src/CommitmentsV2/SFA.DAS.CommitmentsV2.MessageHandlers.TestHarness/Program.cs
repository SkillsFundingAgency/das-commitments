using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness
{
    internal class Program
    {
        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                .AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2MessageHandler);

            IConfigurationRoot configuration = builder.Build();

            var provider = new ServiceCollection()
                .AddOptions()
                .Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2MessageHandler)).BuildServiceProvider();

            var config = provider.GetService<IOptions<CommitmentsV2Configuration>>().Value.NServiceBusConfiguration;
            var isDevelopment = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName) == "LOCAL";

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness")
                .UseAzureServiceBusTransport(isDevelopment, () => config.ServiceBusConnectionString, r => { })
                .UseErrorQueue()
                .UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();

            var endpoint = await Endpoint.Start(endpointConfiguration);

            var testHarness = new TestHarness(endpoint);

            await testHarness.Run();
            await endpoint.Stop();
        }
    }
}
