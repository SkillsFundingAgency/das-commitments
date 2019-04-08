using System;

namespace SFA.DAS.CommitmentsV2.Domain.ValueObjects
{
    public static class Constants
    {
        public static readonly DateTime DasStartDate = new DateTime(2017, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        public const int MinimumAgeAtApprenticeshipStart = 15;
        public const int MaximumAgeAtApprenticeshipStart = 114;
        public const string ServiceName = "SFA.DAS.CommitmentsV2";
    }
}