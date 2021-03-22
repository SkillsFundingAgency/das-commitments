using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.CommitmentsV2.Api.Client.Configuration;

namespace SFA.DAS.CommitmentsV2.Api.Client.TestHarness
{
    class Program
    {

        public static async Task Main(string[] args)

        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false);

            IConfigurationRoot configuration = builder.Build();

            var provider = new ServiceCollection()
                .AddOptions()
                .Configure<CommitmentsClientApiConfiguration>(configuration.GetSection("AzureADClientAuthentication"))
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton<ICommitmentsApiClientFactory>(x=>
                {
                    var config = x.GetService<IOptions<CommitmentsClientApiConfiguration>>().Value;
                    var loggerFactory = x.GetService<ILoggerFactory>();
                    return new CommitmentsApiClientFactory(config, loggerFactory);
                })
                .AddTransient<ICommitmentsApiClient>(x => x.GetService<ICommitmentsApiClientFactory>().CreateClient())
                .AddTransient<TestHarness>()
                .BuildServiceProvider();

            var testHarness = provider.GetService<TestHarness>();

            await testHarness.Run();

        }
    }
}
