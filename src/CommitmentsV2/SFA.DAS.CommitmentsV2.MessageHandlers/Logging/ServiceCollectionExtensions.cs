using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.Logging
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNLog(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging((options) =>
            {
                options.AddFilter(typeof(Program).Namespace, LogLevel.Information);
                options.SetMinimumLevel(LogLevel.Trace);
                options.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });

                NLogConfiguration.ConfigureNLog();
            });

            return serviceCollection;
        }
    }
}
