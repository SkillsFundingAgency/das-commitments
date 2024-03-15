using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers;

public class Program
{
    public static async Task Main(string[] args)
    {
        using var host = CreateHost(args);
        
        var logger = host.Services.GetService<ILogger<Program>>();
        
        logger.LogInformation("SFA.DAS.EmployerAccounts.MessageHandlers starting up ...");

        await host.RunAsync();
    }

    private static IHost CreateHost(string[] args)
    {
        return new HostBuilder()
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration(args)
            .ConfigureDasLogging()
            .ConfigureExternalHandlerServices()
            .Build();
    }
}