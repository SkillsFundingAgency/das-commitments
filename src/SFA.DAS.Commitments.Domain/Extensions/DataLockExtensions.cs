using System;
using System.Globalization;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Extensions
{
    public static class DataLockExtensions
    {
        public static bool UnHandled(this DataLockStatus dl)
        {
            return !dl.IsResolved && dl.Status != Status.Pass && dl.EventStatus != EventStatus.Removed;
        }

        public static bool IsPriceOnly(this DataLockStatus dataLockStatus)
        {
            return (int)dataLockStatus.ErrorCode == (int)DataLockErrorCode.Dlock07;
        }

        public static bool PreviousResolvedPriceDataLocks(this DataLockStatus dataLockStatus)
        {
            return dataLockStatus.ErrorCode == DataLockErrorCode.Dlock07
                && dataLockStatus.Status == Status.Fail
                && dataLockStatus.TriageStatus == TriageStatus.Change
                && dataLockStatus.IsResolved
;        }

        public static bool WithCourseError(this DataLockStatus dataLockStatus)
        {
            return    dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06);
        }

        public static bool WithCourseAndPriceError(this DataLockStatus dataLockStatus)
        {
            var hasCourse = dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06);

            return hasCourse && dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07);
        }

        public static DateTime GetDateFromPriceEpisodeIdentifier(this DataLockStatus dataLockStatus)
        {
            return
            DateTime.ParseExact(dataLockStatus.PriceEpisodeIdentifier.Substring(dataLockStatus.PriceEpisodeIdentifier.Length - 10), "dd/MM/yyyy",
                new CultureInfo("en-GB"));           
        }
    }
}
