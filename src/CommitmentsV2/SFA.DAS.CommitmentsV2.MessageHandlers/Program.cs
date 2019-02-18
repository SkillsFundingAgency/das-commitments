using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.MessageHandlers.NServiceBus;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();

            try
            {
                hostBuilder.UseDasEnvironment()
                    .UseStructureMap()
                    .ConfigureMessageHandlerAppConfiguration(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddOptions();

                        services.ConfigureNServiceBus();
                        services.AddHostedService<NServiceBusHostedService>();

                    })
                    .ConfigureLogging(b => b.AddNLog())
                    .ConfigureContainer<Registry>(IoC.Initialize)
                    .UseConsoleLifetime();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }

            using (var host = hostBuilder.Build())
            {
                await host.RunAsync();
            }
                
        }
    }
}
