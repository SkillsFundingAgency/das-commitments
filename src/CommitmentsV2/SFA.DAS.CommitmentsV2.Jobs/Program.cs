using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Jobs.DependencyResolution;
using SFA.DAS.CommitmentsV2.Jobs.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.Jobs;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHost(args);
        
        var logger = host.Services.GetService<ILogger<Program>>();
        
        logger.LogInformation("SFA.DAS.CommitmentsV2.Jobs starting up ...");

        await host.RunAsync();
    }

    private static IHost CreateHost(string[] args)
    {
        return new HostBuilder()
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration(args)
            .ConfigureDasWebJobs()
            .ConfigureDasLogging()
            .UseConsoleLifetime()
            .ConfigureJobsServices()
            .Build();
    }
}