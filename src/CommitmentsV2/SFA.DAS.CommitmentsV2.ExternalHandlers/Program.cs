using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SFA.DAS.CommitmentsV2.ExternalHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.ExternalHandlers.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = new HostBuilder();

        hostBuilder
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration(args)
            .ConfigureDasLogging()
            .ConfigureExternalHandlerServices();

        using var host = hostBuilder.Build();
        
        await host.RunAsync();
    }
}