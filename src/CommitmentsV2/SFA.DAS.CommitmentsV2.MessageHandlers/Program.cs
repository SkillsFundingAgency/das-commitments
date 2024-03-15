using Microsoft.Extensions.Hosting;
using NLog.Extensions.Logging;
using SFA.DAS.CommitmentsV2.MessageHandlers.DependencyResolution;
using SFA.DAS.CommitmentsV2.MessageHandlers.Extensions;
using SFA.DAS.CommitmentsV2.Startup;

namespace SFA.DAS.CommitmentsV2.MessageHandlers;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var hostBuilder = new HostBuilder();

        hostBuilder
            .UseDasEnvironment()
            .ConfigureDasAppConfiguration(args)
            .UseConsoleLifetime()
            .ConfigureDasLogging()
            .ConfigureMessageHandlerServices();

        using var host = hostBuilder.Build();

        await host.RunAsync();
    }
}