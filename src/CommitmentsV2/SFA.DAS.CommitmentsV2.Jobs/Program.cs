using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Jobs.DependencyResolution;
using SFA.DAS.CommitmentsV2.Jobs.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.Jobs;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = new HostBuilder();

        hostBuilder
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration(args)
            .ConfigureDasWebJobs()
            .ConfigureDasLogging()
            .UseConsoleLifetime()
            .ConfigureJobsServices();

        using var host = hostBuilder.Build();

        await host.RunAsync();
    }
}