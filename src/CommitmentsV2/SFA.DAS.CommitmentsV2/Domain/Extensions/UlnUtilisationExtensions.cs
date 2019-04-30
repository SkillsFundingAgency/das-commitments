using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class UlnUtilisationExtensions
    {
        public static OverlapStatus DetermineOverlap(this UlnUtilisation ulnUtilisation, DateRange range)
        {
            //End on start date (effectively deleted) should be ignored
            if (ulnUtilisation.DateRange.IsZeroDays)
            {
                return OverlapStatus.None;
            }

            var overlapsStart = ulnUtilisation.DateRange.Contains(range.From);
            var overlapsEnd = ulnUtilisation.DateRange.Contains(range.To);

            //Contained
            if (overlapsStart && overlapsEnd)
            {
                return OverlapStatus.DateWithin;
            }

            //Overlap start date
            if (overlapsStart)
            {
                return OverlapStatus.OverlappingStartDate;
            }

            //Overlap end date
            if (overlapsEnd)
            {
                return OverlapStatus.OverlappingEndDate;
            }

            //Straddle
            if (IsApprenticeshipDateStraddle(range, ulnUtilisation.DateRange))
            {
                return OverlapStatus.DateEmbrace;
            }

            return OverlapStatus.None;
        }

        private static bool IsApprenticeshipDateStraddle(DateRange candidate, DateRange target)
        {
            //straightforward case - clear straddle
            if(candidate.From.IsBeforeMonth(target.From) && candidate.To.IsAfterMonth(target.To))           
            {
                return true;
            }

            //Case where active apprenticeship is single-month and not a clear straddle, cannot overlap
            if(target.IsZeroDays)
            {
                return false;
            }

            //Case where timespans are identical
            if (candidate.From.IsSameMonthAndYear(target.From) && candidate.To.IsSameMonthAndYear(target.To))
            {
                //Then single month apprenticeships do not overlap
                return !candidate.IsZeroDays;
            }

            //If the apprenticeships share a start date
            if (candidate.From.IsSameMonthAndYear(target.From))
            {
                //The if is a single month then do not overlap
                return !candidate.IsZeroDays;
            }

            //If they share an end date
            if (candidate.To.IsSameMonthAndYear(target.To))
            {
                //Then if is a single month then do not overlap
                return !candidate.IsZeroDays;
            }

            return false;
        }
    }
}
