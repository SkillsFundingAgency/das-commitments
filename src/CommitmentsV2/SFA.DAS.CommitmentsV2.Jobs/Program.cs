using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Jobs.DependencyResolution;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.Jobs
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder();
            try
            {
                hostBuilder
                    //.UseDasEnvironment()
                    //.MessageHandlerAppConfiguration(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                    })
                    .ConfigureLogging(b => b.AddNLog())
                    .UseConsoleLifetime()
                    .UseStructureMap()
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
