using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.MessageHandlers.Configuration;
using SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution;
using StructureMap;

namespace SFA.DAS.CommitmentsV2.MessageHandlers
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            //var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            //logger.Info("Starting up host");


            var host = new HostBuilder()
                .ConfigureMessageHandlerAppConfiguration(args)
                .ConfigureServices((hostContext, services) =>
                {
                    var config = services.BuildServiceProvider().GetService<IConfiguration>();
                    var section = config.GetSection("SFA.DAS.Commitments");

                    services.AddOptions();
                    services.Configure<MyClass>(section);


                    var a = services.BuildServiceProvider().GetService<IOptions<MyClass>>();
                    services.AddHostedService<MessageHandlerService>();

                })
                .ConfigureLogging(b => b.AddNLog())
                .UseStructureMap()
                .ConfigureContainer<Registry>(IoC.Initialize)
                .UseConsoleLifetime()
                .Build();

            using (host)
            {
                await host.RunAsync();
            }
                
        }
    }

    public class MyClass
    {
        public string DatabaseConnectionString { get; set; }
    }
}
