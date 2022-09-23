using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services.DataLockUpdate
{
    public static class DataLockUpdaterTestData
    {
        public static List<DataLockStatus> GetData(int year)
        {
            return new List<DataLockStatus>
            {
                new DataLockStatus
                {
                    DataLockEventId = 1,
                    ApprenticeshipId = 121,
                    PriceEpisodeIdentifier = "25-6-01/06/2016",
                    IlrEffectiveFromDate = new DateTime(year, 6, 1), // in Acc.Yr 2015/16
                    ErrorCode = DataLockErrorCode.Dlock03
                },
                new DataLockStatus
                {
                    DataLockEventId = 2,
                    ApprenticeshipId = 122,
                    PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(year, 6, 1), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock03
                },
                new DataLockStatus
                {
                    DataLockEventId = 3,
                    ApprenticeshipId = 123,
                    PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(year, 6, 1), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock04,
                    IsExpired = true, // but is already expired
                    Expired = DateTime.MaxValue // but already expired
                },
                new DataLockStatus
                {
                    DataLockEventId = 4,
                    ApprenticeshipId = 124,
                    PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(year, 6, 2), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock05
                },
                 new DataLockStatus
                {
                    DataLockEventId = 5,
                    ApprenticeshipId = 125,
                    PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(year, 6, 3), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock06
                },
                new DataLockStatus
                {
                    DataLockEventId = 6,
                    ApprenticeshipId = 126,
                    PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(year, 6, 4), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock07
                },
                new DataLockStatus
                {
                    DataLockEventId = 7,
                    ApprenticeshipId = 127,
                    PriceEpisodeIdentifier = "25-6-01/05/2017",
                    IlrEffectiveFromDate = new DateTime(year, 6, 5), // in Acc.Yr 2016/17
                    ErrorCode = DataLockErrorCode.Dlock03 |
                                DataLockErrorCode.Dlock04 |
                                DataLockErrorCode.Dlock05 |
                                DataLockErrorCode.Dlock06 |
                                DataLockErrorCode.Dlock07
                },
            };
        }
    }
}