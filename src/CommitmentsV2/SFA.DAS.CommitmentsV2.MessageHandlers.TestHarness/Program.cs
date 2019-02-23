using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.EmployerAccounts.Messages.Events;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness
{
    internal class Program
    {
        public static async Task Main()
        {
            var endpointConfiguration = new EndpointConfiguration("SFA.DAS.CommitmentsV2.MessageHandlers.TestHarness")
                .UseAzureServiceBusTransport(true, () => "", r => { })
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
