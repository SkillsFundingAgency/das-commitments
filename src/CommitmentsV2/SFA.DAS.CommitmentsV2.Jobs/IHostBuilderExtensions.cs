using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Extensions.Hosting;

namespace SFA.DAS.CommitmentsV2.Jobs
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureDasWebJobs(this IHostBuilder builder)
        {
            builder.ConfigureWebJobs(b =>
            {
                b.AddTimers( );
            });

#pragma warning disable 618
            builder.ConfigureServices(s => s.AddSingleton<IWebHookProvider>(p => null));
#pragma warning restore 618

            return builder;
        }
    }
}
