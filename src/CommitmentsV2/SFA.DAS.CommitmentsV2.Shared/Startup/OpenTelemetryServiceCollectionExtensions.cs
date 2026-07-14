using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SFA.DAS.CommitmentsV2.Shared.Startup;

public static class OpenTelemetryServiceCollectionExtensions
{
    public static IServiceCollection AddDasOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];

        if (!string.IsNullOrEmpty(connectionString))
        {
            services.AddOpenTelemetry().UseAzureMonitor(options => options.ConnectionString = connectionString);
        }

        return services;
    }
}
