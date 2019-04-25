using System;

namespace SFA.DAS.CommitmentsV2.Domain
{
    public static class Constants
    {
        public static readonly DateTime DasStartDate = new DateTime(2017, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        public const int MinimumAgeAtApprenticeshipStart = 15;
        public const int MaximumAgeAtApprenticeshipStart = 115;
        public const int MaximumApprenticeshipCost = 100000;
        public const string ServiceName = "SFA.DAS.CommitmentsV2";
    }
}
