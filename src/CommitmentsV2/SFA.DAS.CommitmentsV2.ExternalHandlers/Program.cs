using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Caching;
using SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.ExternalHandlers.NServiceBus;
using SFA.DAS.CommitmentsV2.Startup;
using StructureMap;

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
                    .UseConsoleLifetime()
                    .UseStructureMap()
                    .ConfigureServices((c, s) => s
                        .AddDasDistributedMemoryCache(c.Configuration, c.HostingEnvironment.IsDevelopment())
                        .AddMemoryCache()
                        .AddNServiceBus())
                    .ConfigureContainer<Registry>(IoC.Initialize);

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
