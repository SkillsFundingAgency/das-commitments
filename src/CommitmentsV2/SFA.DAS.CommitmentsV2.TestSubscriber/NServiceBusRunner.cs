using System;
using System.Data.Common;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.ObjectBuilder.Common;
using SFA.DAS.NServiceBus;
using SFA.DAS.NServiceBus.AzureServiceBus;
using SFA.DAS.NServiceBus.NewtonsoftJsonSerializer;
using SFA.DAS.NServiceBus.NLog;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    public class NServiceBusRunner : INServiceBusRunner
    {
        public Task StartNServiceBusBackgroundTask(string connectionString)
        {
            return StartNServiceBus(connectionString)

                .ContinueWith(task =>
                {
                    if (!task.IsCompletedSuccessfully)
                    {
                        Console.WriteLine("An error occurred starting NServiceBus");
                        task.Exception.DumpException();
                        throw new InvalidOperationException("Cannot continue because nservicebus could not be started");
                    }

                    Console.WriteLine("NServiceBus started. Press escape to exit...");

                    while (Console.ReadKey(true).Key != ConsoleKey.Escape)
                    {
                    }

                    return task.Result;
                })

                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        StopNServiceBus(task.Result);
                    }
                });
        }

        private async Task<IEndpointInstance> StartNServiceBus(string connectionString)
        {
            Console.WriteLine("Starting NServiceBus...");
            var endpointConfiguration = new EndpointConfiguration(Constants.NameSpace);

            UseDasMessageConventions(endpointConfiguration);

            endpointConfiguration
                .UseAzureServiceBusTransport(string.IsNullOrWhiteSpace(connectionString), () => connectionString, r => { })
                .UseNewtonsoftJsonSerializer()
                ;

            var endpointInstance = await Endpoint
                .Start(endpointConfiguration)
                .ConfigureAwait(false);

            return endpointInstance;
        }

        private Task StopNServiceBus(IEndpointInstance endpointInstance)
        {
            return endpointInstance.Stop();
        }

        private EndpointConfiguration UseDasMessageConventions(EndpointConfiguration config)
        {
            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("SFA.DAS.CommitmentsV2.Messages.Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.StartsWith("SFA.DAS.CommitmentsV2.Messages.Events"));
            return config;
        }
    }
}