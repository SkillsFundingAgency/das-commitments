﻿using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.ApplicationInsights;
using NLog.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDasLogging(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureLogging((context, loggingBuilder) =>
        {
            var connectionString = context.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
            if (!string.IsNullOrEmpty(connectionString))
            {
                loggingBuilder.AddNLog(context.HostingEnvironment.IsDevelopment()
                    ? "nlog.development.config"
                    : "nlog.config");
                loggingBuilder.AddApplicationInsightsWebJobs(o => o.ConnectionString = connectionString);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>(string.Empty, LogLevel.Information);
                loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Information);
            }

            loggingBuilder.AddConsole();
        });

        return hostBuilder;
    }
}