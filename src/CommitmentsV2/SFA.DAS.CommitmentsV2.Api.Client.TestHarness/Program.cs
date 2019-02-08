using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.CommitmentsV2.Api.Client.Http;
using SFA.DAS.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client.TestHarness
{
    class Program
    {

        public static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false);

            IConfigurationRoot configuration = builder.Build();
            var section = configuration.GetSection("AzureADAuthentication");

            var provider = new ServiceCollection()
                .AddOptions()
                .Configure<AzureActiveDirectoryClientConfiguration>(configuration.GetSection("AzureADClientAuthentication"))
                .AddLogging()
                .AddSingleton<IHttpClientFactory>(x=>
                {
                    var config = x.GetService<IOptions<AzureActiveDirectoryClientConfiguration>>().Value;
                    return new HttpClientFactory(config);
                })
                .AddTransient<IRestHttpClient>(x =>
                {
                    var httpClient = x.GetService<IHttpClientFactory>().CreateHttpClient();
                    return new RestHttpClient(httpClient);
                })
                .AddTransient<CommitmentsApiClient>()
                .AddTransient<TestHarness>()
                .BuildServiceProvider();

            var testHarness = provider.GetService<TestHarness>();

            await testHarness.Run();

        }

    }
}
