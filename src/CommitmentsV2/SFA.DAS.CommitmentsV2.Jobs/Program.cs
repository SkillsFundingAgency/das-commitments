using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Jobs.DependencyResolution;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.Jobs;

public class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = new HostBuilder();
        try
        {
            hostBuilder
                .UseDasEnvironment()
                .ConfigureDasAppConfiguration(args)
                .ConfigureDasWebJobs()
                .ConfigureLogging(b => b.AddNLog())
                .UseConsoleLifetime()
                .ConfigureJobsServices();

            using var host = hostBuilder.Build();
            
            await host.RunAsync();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
            throw;
        }
    }
}