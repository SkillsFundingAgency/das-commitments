using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using SFA.DAS.Commitments.Support.SubSite.Extentions;
using StructureMap.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureDasAppConfiguration()
                .UseNLog()
                .UseStructureMap()
                .UseStartup<Startup>();
    }
}