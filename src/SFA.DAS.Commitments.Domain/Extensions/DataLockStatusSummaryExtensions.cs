using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Extensions
{
    public static class DataLockStatusSummaryExtensions
    {
        public static bool IsPriceOnly(this DataLockStatusSummary dataLockStatus)
        {
            return (int)dataLockStatus.ErrorCode == (int)DataLockErrorCode.Dlock07;
        }

        public static bool WithCourseError(this DataLockStatusSummary dataLockStatus)
        {
            return dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06);
        }

        public static bool WithCourseAndPriceError(this DataLockStatusSummary dataLockStatus)
        {
            var hasCourse = dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                            || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                            || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                            || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06);

            return hasCourse && dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock07);
        }
    }
}
