using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using NLog.Web;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using StructureMap.AspNetCore;
using System;

namespace SFA.DAS.CommitmentsV2.Api
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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureDasAppConfiguration()
                .UseApplicationInsights()
                .UseNLog()
                .UseStructureMap()
                .UseStartup<Startup>()
        ;
    }
}