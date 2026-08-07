using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace SFA.DAS.CommitmentsV2.Shared.Startup;

public static class HostBuilderExtensions
{
    public static IHostBuilder ConfigureDasOpenTelemetry(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((context, services) =>
        {
            services.AddDasOpenTelemetry(context.Configuration);
        });

        hostBuilder.ConfigureLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter<OpenTelemetryLoggerProvider>(string.Empty, LogLevel.Information);
            loggingBuilder.AddFilter<OpenTelemetryLoggerProvider>("Microsoft", LogLevel.Information);
            loggingBuilder.AddConsole();
        });

        return hostBuilder;
    }
}
