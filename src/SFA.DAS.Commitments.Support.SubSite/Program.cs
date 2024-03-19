using Microsoft.Extensions.Hosting;
using NLog.Web;
using System;
using Microsoft.AspNetCore.Hosting;
using SFA.DAS.Commitments.Support.SubSite.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.Commitments.Support.SubSite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var logger = NLogBuilder.ConfigureNLog(environment == "Development" ? "nlog.Development.config" : "nlog.config").GetCurrentClassLogger();
            logger.Info("Starting up host");

            CreateWebHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureDasAppConfiguration()
                .UseNLog()
                .UseStructureMap()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}