using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.TestSubscriber
{
    class Program
    {
        private static readonly string AppName = $"{typeof(Program).Namespace}";

        static Task Main(string[] args)
        {
            return new Program().Run(args);
        }

        private async Task Run(string[] args)
        {
            Console.Title = AppName;
            Console.WriteLine("Starting...");
            var endpointInstance = await StartNServiceBus();

            Console.WriteLine("Press escape to exit...");

            while (Console.ReadKey(true).Key != ConsoleKey.Escape)
            {
            }

            await StopNServiceBus(endpointInstance);
        }

        private async Task<IEndpointInstance> StartNServiceBus()
        {
            var endpointConfiguration = new EndpointConfiguration(AppName);

            UseDasMessageConventions(endpointConfiguration);

            endpointConfiguration.UseTransport<LearningTransport>();
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();

            var endpointInstance = await Endpoint
                                            .Start(endpointConfiguration)
                                            .ConfigureAwait(false);

            return endpointInstance;
        }

        private Task StopNServiceBus(IEndpointInstance endpointInstance)
        {
            return endpointInstance.Stop();
        }

        public static EndpointConfiguration UseDasMessageConventions(EndpointConfiguration config)
        {
            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.StartsWith("SFA.DAS.CommitmentsV2.Messages.Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.StartsWith("SFA.DAS.CommitmentsV2.Messages.Events"));
            return config;
        }
    }
}
