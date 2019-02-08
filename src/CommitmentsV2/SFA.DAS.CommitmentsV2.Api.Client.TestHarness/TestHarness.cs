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
    public class TestHarness
    {
        private readonly ICommitmentsApiClient _client;
        private readonly ILogger _logger;

        public TestHarness(ICommitmentsApiClient client) //, ILogger logger)
        {
            _client = client;
            //_logger = logger;
        }

        public async Task Run()
        {

            var key = "";

            while (key != "x")
            {
                Console.Clear();
                Console.WriteLine("Test Options");
                Console.WriteLine("------------");
                Console.WriteLine("A - Run Heath-Check");
                Console.WriteLine("B - Run Secure endpoint");
                Console.WriteLine("X - Exit");
                Console.WriteLine("Press [Key] for Test Option");
                key = Console.ReadKey().Key.ToString().ToLower();
                try
                {
                    switch (key)
                    {
                        case "a":
                            var value = await _client.HealthCheck();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine($"Calling HeaithCheck endpoint - Result {value}");
                            Console.WriteLine();

                            break;
                        case "b":
                            var result = await _client.SecureCheck();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine();
                            Console.WriteLine($"Calling Secure endpoint - Result {result}");
                            Console.WriteLine();

                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }

                Console.WriteLine("Press anykey to return to menu");
                Console.ReadKey();

            }



            //_logger.LogDebug("All done!");
        }

    }
}
