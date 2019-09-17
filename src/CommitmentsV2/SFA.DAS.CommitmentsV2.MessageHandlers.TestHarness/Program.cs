using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness
{
    internal class Program
    {
        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                .AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2);

            IConfigurationRoot configuration = builder.Build();

            var provider = new ServiceCollection()
                .AddOptions()
                .Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2)).BuildServiceProvider();

            var config = provider.GetService<IOptions<CommitmentsV2Configuration>>().Value.NServiceBusConfiguration;
            var isDevelopment = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName) == "LOCAL";

            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness")
                .UseErrorQueue()
                .UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();

            if (isDevelopment)
            {
                endpointConfiguration.UseLearningTransport();
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(config.ServiceBusConnectionString);
            }

            var endpoint = await Endpoint.Start(endpointConfiguration);

            var testHarness = new TestHarness(endpoint);

            await testHarness.Run();
            await endpoint.Stop();
        }
    }
}
