using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SFA.DAS.NServiceBus.Configuration;
using SFA.DAS.NServiceBus.Configuration.AzureServiceBus;
using SFA.DAS.NServiceBus.Configuration.NewtonsoftJsonSerializer;

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
            var endpointConfiguration = new EndpointConfiguration(Constants.NameSpace)
                .UseNewtonsoftJsonSerializer()
                .UseInstallers();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                endpointConfiguration.UseLearningTransport();
            }
            else
            {
                endpointConfiguration.UseAzureServiceBusTransport(connectionString);
            }
            
            var defaultFactory = LogManager.Use<DefaultFactory>();
            defaultFactory.Level(LogLevel.Debug);

            var endpointInstance = await Endpoint
                .Start(endpointConfiguration)
                .ConfigureAwait(false);

            return endpointInstance;
        }

        private Task StopNServiceBus(IEndpointInstance endpointInstance)
        {
            return endpointInstance.Stop();
        }
    }
}