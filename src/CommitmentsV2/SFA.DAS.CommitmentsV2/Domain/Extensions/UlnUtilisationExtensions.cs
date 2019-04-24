using System;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class UlnUtilisationExtensions
    {
        public static OverlapStatus DetermineOverlap(this UlnUtilisation ulnUtilisation, DateTime startDate, DateTime endDate)
        {
            var overlapsStart = IsApprenticeshipDateBetween(startDate, ulnUtilisation.StartDate, ulnUtilisation.EndDate);
            var overlapsEnd = IsApprenticeshipDateBetween(endDate, ulnUtilisation.StartDate, ulnUtilisation.EndDate);

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
            if (IsApprenticeshipDateStraddle(startDate, endDate, ulnUtilisation.StartDate, ulnUtilisation.EndDate))
            {
                return OverlapStatus.DateEmbrace;
            }

            return OverlapStatus.None;
        }

        private static bool IsApprenticeshipDateBetween(DateTime dateToCheck, DateTime dateFrom, DateTime dateTo)
        {
            return IsApprenticeshipDateAfter(dateToCheck, dateFrom) && IsDateBefore(dateToCheck, dateTo);
        }

        private static bool IsDateBefore(DateTime date1, DateTime date2)
        {
            if (date1.Year < date2.Year) return true;
            if (date1.Year > date2.Year) return false;
            if (date1.Month < date2.Month) return true;
            if (date1.Month > date2.Month) return false;
            return false;
        }

        private static bool IsApprenticeshipDateAfter(DateTime date1, DateTime date2)
        {
            return IsDateBefore(date2, date1);
        }

        private static bool IsApprenticeshipDateStraddle(DateTime date1Start, DateTime date1End, DateTime date2Start, DateTime date2End)
        {
            //straightforward case - clear straddle
            if (IsDateBefore(date1Start, date2Start) && IsApprenticeshipDateAfter(date1End, date2End))
            {
                return true;
            }

            //Case where active apprenticeship is single-month, cannot overlap
            if (IsSameMonthAndYear(date2Start, date2End))
            {
                return false;
            }

            //Case where timespans are identical
            if (IsSameMonthAndYear(date1Start, date2Start) && IsSameMonthAndYear(date1End, date2End))
            {
                //Then single month apprenticeships do not overlap
                if (IsSameMonthAndYear(date1Start, date1End))
                {
                    return false;
                }

                return true;
            }

            //If the apprenticeships share a start date
            if (IsSameMonthAndYear(date1Start, date2Start))
            {
                //The if is a single month then do not overlap
                if (IsSameMonthAndYear(date1Start, date1End))
                {
                    return false;
                }

                return true;
            }

            //If they share an end date
            if (IsSameMonthAndYear(date1End, date2End))
            {
                //Then if is a single month then do not overlap
                if (IsSameMonthAndYear(date1Start, date1End))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static bool IsSameMonthAndYear(DateTime date1, DateTime date2)
        {
            return date1.Month == date2.Month && date1.Year == date2.Year;
        }
    }
}
