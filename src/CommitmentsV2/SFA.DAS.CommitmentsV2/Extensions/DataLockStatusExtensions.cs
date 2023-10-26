using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Extensions
{
    public static class DataLockStatusExtensions
    {
        public static bool IsDuplicate(this DataLockStatus item1, DataLockStatus item2)
        {
            return (item1.ApprenticeshipId == item2.ApprenticeshipId
                    && item1.PriceEpisodeIdentifier == item2.PriceEpisodeIdentifier
                    && item1.Status == item2.Status
                    && item1.IlrActualStartDate == item2.IlrActualStartDate
                    && item1.IlrEffectiveFromDate == item2.IlrEffectiveFromDate
                    && item1.IlrPriceEffectiveToDate == item2.IlrPriceEffectiveToDate
                    && item1.IlrTotalCost == item2.IlrTotalCost
                    && item1.ErrorCode == item2.ErrorCode
                    && item1.IlrTrainingType == item2.IlrTrainingType
                    );
        }
    }
}
