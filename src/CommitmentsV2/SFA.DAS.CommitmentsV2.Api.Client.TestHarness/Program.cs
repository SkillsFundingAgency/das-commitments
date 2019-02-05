using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Client.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client.TestHarness
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("CommitmentV2 Api Client TestHarness");

            Task.Run(Test).Wait();

        }

        private static async Task Test()
        {
            //setup our DI
            var serviceProvider = new ServiceCollection()
                .AddLogging()
                //.AddSingleton<IFooService, FooService>()
                //.AddSingleton<IBarService, BarService>()
                .BuildServiceProvider();

            //configure console logging
            serviceProvider
                .GetService<ILoggerFactory>()
                .AddConsole(LogLevel.Debug);

            var logger = serviceProvider.GetService<ILoggerFactory>()
                .CreateLogger<Program>();
            logger.LogDebug("Starting");



            var factory = new HttpClientFactory(new AzureActiveDirectoryClientConfiguration
            {
                ApiBaseUrl = "https://localhost:5001/"
            });

            var client = factory.CreateHttpClient();
            var restClient = new RestHttpClient(client);

            var sut = new CommitmentV2ApiClient(restClient);
            var key = "";

            while (key != "x")
            {
                Console.Clear();
                Console.WriteLine("Test Options");
                Console.WriteLine("------------");
                Console.WriteLine("A - Run Heath-Check");
                Console.WriteLine("X - Exit");
                Console.WriteLine("Press [Key] for Test Option");
                key = Console.ReadKey().Key.ToString().ToLower();

                switch (key)
                {
                    case "a":
                        var value = await sut.HealthCheck();
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine($"Calling HeaithCheck endpoint - Result {value}");
                        Console.WriteLine();
                        Console.WriteLine("Press anykey to return to menu");
                        Console.ReadKey();

                        break;
                }

            }


            //do the actual work here
            //var bar = serviceProvider.GetService<IBarService>();
            //bar.DoSomeRealWork();

            logger.LogDebug("All done!");
        }


    }
}
