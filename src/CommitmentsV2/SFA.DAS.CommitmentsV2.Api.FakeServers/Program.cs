using System;

namespace SFA.DAS.CommitmentsV2.Api.FakeServers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProviderRelationshipsAPIMockBuilder.Create(34900)
                .Setup()
                .Build();

            AccountsAPIMockBuilder.Create(34901)
                .Setup()
                .Build();

            Console.WriteLine("ProviderRelationships API running on port 34900");
            Console.WriteLine("Accounts API running on port 34901");
            Console.WriteLine("Press any key to stop ...");
            Console.ReadKey();
        }
    }
}
