using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.Api.Client.TestHarness
{
    public class TestHarness
    {
        private readonly ICommitmentsApiClient _client;
        private readonly ILogger<TestHarness> _logger;

        public TestHarness(ICommitmentsApiClient client, ILogger<TestHarness> logger)
        {
            _client = client;
            _logger = logger;
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
                Console.WriteLine("B - Call Secure endpoint");
                Console.WriteLine("C - Call Secure endpoint for Provider");
                Console.WriteLine("D - Call Secure endpoint for Employer");
                Console.WriteLine("X - Exit");
                Console.WriteLine("Press [Key] for Test Option");
                key = Console.ReadKey().Key.ToString().ToLower();

                string result = null;

                try
                {
                    switch (key)
                    {
                        case "a":
                            await _client.Ping();
                            Console.WriteLine();
                            Console.WriteLine($"Calling Ping endpoint - Result OK");
                            break;
                        case "b":
                            result = await _client.SecureCheck();
                            Console.WriteLine();
                            Console.WriteLine($"Calling Secure endpoint - Result {result}");
                            break;
                        case "c":
                            result = await _client.SecureProviderCheck();
                            Console.WriteLine();
                            Console.WriteLine($"Calling Secure Provider endpoint - Result {result}");
                            break;
                        case "d":
                            result = await _client.SecureEmployerCheck();
                            Console.WriteLine();
                            Console.WriteLine($"Calling Secure Employer endpoint - Result {result}");
                            break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.WriteLine("Press anykey to return to menu");
                Console.ReadKey();
            }
        }
    }
}
