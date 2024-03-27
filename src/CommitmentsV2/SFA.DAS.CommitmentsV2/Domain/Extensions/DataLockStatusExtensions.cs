using System.Globalization;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class DataLockStatusExtensions
    {
        public static bool UnHandled(this DataLockStatus dl)
        {
            return !dl.IsResolved && dl.Status != Status.Pass && !dl.IsExpired && dl.EventStatus != EventStatus.Removed;
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
                && dataLockStatus.IsResolved;
        }

        public static bool WithCourseError(this DataLockStatus dataLockStatus)
        {
            return    dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06);
        }

        public static DateTime GetDateFromPriceEpisodeIdentifier(this DataLockStatus dataLockStatus)
        {
            return DateTime.ParseExact(
                dataLockStatus.PriceEpisodeIdentifier.Substring(dataLockStatus.PriceEpisodeIdentifier.Length - 10), 
                "dd/MM/yyyy",
                new CultureInfo("en-GB"));           
        }

        public static bool WithCourseAndPriceError(this DataLockStatus dataLockStatus)
        {
            return dataLockStatus.WithCourseError() 
                   && dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07);
        }

        
    }
}
