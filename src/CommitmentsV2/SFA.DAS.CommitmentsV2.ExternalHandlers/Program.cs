using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            try
            {
                hostBuilder
                    .UseDasEnvironment()
                    .ConfigureDasAppConfiguration(args)
                    .ConfigureLogging(b => b.AddNLog())
                    .ConfigureExternalHandlerServices();

                using (var host = hostBuilder.Build())
                {
                    await host.RunAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }
    }
}
