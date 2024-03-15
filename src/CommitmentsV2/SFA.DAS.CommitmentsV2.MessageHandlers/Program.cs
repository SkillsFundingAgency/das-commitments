using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.MessageHandlers.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.MessageHandlers;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHost(args);
        
        var logger = host.Services.GetService<ILogger<Program>>();
        
        logger.LogInformation("SFA.DAS.CommitmentsV2.MessageHandlers starting up ...");

        await host.RunAsync();
    }

    private static IHost CreateHost(string[] args)
    {
        return new HostBuilder()
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration(args)
            .UseConsoleLifetime()
            .ConfigureDasLogging()
            .ConfigureMessageHandlerServices()
            .Build();
    }
}