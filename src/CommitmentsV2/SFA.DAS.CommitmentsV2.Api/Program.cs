using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using StructureMap.AspNetCore;

namespace SFA.DAS.CommitmentsV2.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
            logger.Info("Starting up host");

            CreateWebHostBuilder(args).Build().Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureDasAppConfiguration()
                .ConfigureKestrel(options => options.AddServerHeader = false)
                .UseStructureMap()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .UseNLog()
        ;
    }
}
