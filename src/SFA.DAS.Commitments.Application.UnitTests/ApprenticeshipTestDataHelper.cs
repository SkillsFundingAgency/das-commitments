using System;

namespace SFA.DAS.Commitments.Application.UnitTests
{
    internal static class ApprenticeshipTestDataHelper
    {
        private static Random _rnd = new Random();

        public static string CreateValidULN()
        {
            return "1" + _rnd.Next(1000, 99999).ToString("D9");
        }
    }
}
