using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Extensions
{
    public static class DataLockExtensions
    {
        public static bool UnHandeled(this DataLockStatus dl)
        {
            return !dl.IsResolved && dl.Status != Status.Pass;
        }

        public static bool IsPriceOnly(this DataLockStatus dataLockStatus)
        {
            return dataLockStatus.ErrorCode == DataLockErrorCode.Dlock07;
        }

        public static bool WithCourseError(this DataLockStatus dataLockStatus)
        {
            return    dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock03)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock04)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock05)
                   || dataLockStatus.ErrorCode.HasFlag(DataLockErrorCode.Dlock06);
        }
    }
}
