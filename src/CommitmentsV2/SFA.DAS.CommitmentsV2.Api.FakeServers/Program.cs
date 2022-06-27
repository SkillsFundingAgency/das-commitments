using System;

namespace SFA.DAS.CommitmentsV2.Api.FakeServers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ProviderRelationshipsAPIMockBuilder.Create(44900)
                .Setup()
                .Build();

            ReservationsAPIMockBuilder.Create(44901)
                .Setup()
                .Build();

            AccountsAPIMockBuilder.Create(44902)
                .Setup()
                .Build();

            Console.WriteLine("ProviderRelationships API running on port 44900");
            Console.WriteLine("Reservations API running on port 44901");
            Console.WriteLine("Accounts API running on port 44902");
            Console.WriteLine("Press any key to stop ...");
            Console.ReadKey();
        }
    }
}
