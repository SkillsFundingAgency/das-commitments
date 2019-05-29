﻿using System;

namespace SFA.DAS.CommitmentsV2.Domain
{
    public static class Constants
    {
        public static readonly DateTime DasStartDate = new DateTime(2017, 5, 1, 0, 0, 0, DateTimeKind.Utc);
        public const int MinimumAgeAtApprenticeshipStart = 15;
        public const int MaximumAgeAtApprenticeshipStart = 115;
        public const int MaximumApprenticeshipCost = 100000;
        public const string ServiceName = "SFA.DAS.CommitmentsV2";
        public const string IntegrationTestEnvironment = "IntegrationTest";

        /// <summary>
        ///     The maximum lengths of various fields (as defined in the database).
        /// </summary>
        public static class FieldLengths
        {
            public const int FirstName = 100;
            public const int LastName = 100;
            public const int CourseCode = 20;
            public const int ProviderReference = 50;
            public const int Uln = 50;
        }
    }
}
