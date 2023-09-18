using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;
using SFA.DAS.Payments.ProviderPayments.Messages;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness
{
    internal class Program
    {
        private const string EndpointName = "SFA.DAS.CommitmentsV2.TestHarness";

        public static async Task Main()
        {
            var builder = new ConfigurationBuilder()
                .AddAzureTableStorage(CommitmentsConfigurationKeys.CommitmentsV2);

            var configuration = builder.Build();
            
            var provider = new ServiceCollection()
                .AddOptions()
                .Configure<CommitmentsV2Configuration>(configuration.GetSection(CommitmentsConfigurationKeys.CommitmentsV2)).BuildServiceProvider();

            var config = provider.GetService<IOptions<CommitmentsV2Configuration>>().Value.NServiceBusConfiguration;
            var isDevelopment = Environment.GetEnvironmentVariable(EnvironmentVariableNames.EnvironmentName) == "LOCAL";

            var endpointConfiguration = new EndpointConfiguration(EndpointName)
                .UseErrorQueue($"{EndpointName}-errors")
                .UseInstallers()
                .UseMessageConventions()
                .UseNewtonsoftJsonSerializer();

            endpointConfiguration.Conventions().DefiningEventsAs(t =>
                        t == typeof(RecordedAct1CompletionPayment) ||
                        t == typeof(EntityStateChangedEvent) ||
                        t.Name.EndsWith("Event"));

            if (isDevelopment)
            {
                endpointConfiguration.UseLearningTransport(s => s.AddRouting());
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(config.SharedServiceBusEndpointUrl, s => s.AddRouting());
            }

            var endpoint = await Endpoint.Start(endpointConfiguration);

            var testHarness = new TestHarness(endpoint);

            await testHarness.Run();
            await endpoint.Stop();
        }
    }
}
