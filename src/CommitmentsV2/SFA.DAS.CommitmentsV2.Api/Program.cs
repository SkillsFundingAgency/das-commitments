﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using NLog.Web;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.NServiceBus.Configuration.MicrosoftDependencyInjection;

namespace SFA.DAS.CommitmentsV2.Api;

public static class Program
{
    public static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var logger = NLogBuilder.ConfigureNLog(environment == "Development" ? "nlog.Development.config" : "nlog.config").GetCurrentClassLogger();
        logger.Info("Starting up host");

        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseNServiceBusContainer()
            .ConfigureDasAppConfiguration()
            .UseNLog()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.ConfigureKestrel(c => c.AddServerHeader = false)
                .UseStartup<Startup>();
            });
}