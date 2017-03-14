using System;
using SFA.DAS.Commitments.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public class ApprenticeshipOverlapRules : IApprenticeshipOverlapRules
    {
        //todo: return reason
        public bool IsOverlap(OverlappingApprenticeshipRequest request, ApprenticeshipResult apprenticeship)
        {
            if(!request.Uln.Equals(apprenticeship.Uln, StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (request.ExcludeApprenticeshipId == apprenticeship.Id)
            {
                return false;
            }

            //Get the appropriate dates for the apprenticeship
            //Additional logic to select other dates based on status can go here
            var apprenticeshipStartDate = apprenticeship.StartDate;
            var apprenticeshipEndDate = apprenticeship.EndDate;

            var overlapsStart = IsApprenticeshipDateBetween(request.DateFrom, apprenticeshipStartDate, apprenticeshipEndDate);
            var overlapsEnd = IsApprenticeshipDateBetween(request.DateTo, apprenticeshipStartDate, apprenticeshipEndDate);

            //Contained
            if (overlapsStart && overlapsEnd)
            {
                return true;
            }
            //Overlap start date
            else if (overlapsStart)
            {
                return true;
            }
            //Overlap end date
            else if (overlapsEnd)
            {
                return true;
            }
            //Straddle
            else if (IsApprenticeshipDateStraddle(request.DateFrom, request.DateTo, apprenticeshipStartDate, apprenticeshipEndDate))
            {
                return true;
            }

            return false;
        }

        public static bool IsApprenticeshipDateBetween(DateTime dateToCheck, DateTime dateFrom, DateTime dateTo)
        {
            return (IsApprenticeshipDateAfter(dateToCheck, dateFrom) && IsApprenticeshipDateBefore(dateToCheck, dateTo));
        }

        private static bool IsApprenticeshipDateBefore(DateTime dateToCheck, DateTime checkAgainstDate)
        {
            if (dateToCheck.Year < checkAgainstDate.Year) return true;
            if (dateToCheck.Year > checkAgainstDate.Year) return false;
            if (dateToCheck.Month < checkAgainstDate.Month) return true;
            if (dateToCheck.Month > checkAgainstDate.Month) return false;
            return false;
        }

        private static bool IsApprenticeshipDateAfter(DateTime dateToCheck, DateTime checkAgainstDate)
        {
            return IsApprenticeshipDateBefore(checkAgainstDate, dateToCheck);
        }

        private static bool IsApprenticeshipDateStraddle(DateTime date1Start, DateTime date1End, DateTime date2Start,
            DateTime date2End)
        {
            //does date 1 straddle date 2?
            if (IsApprenticeshipDateBefore(date1Start, date2Start) &&
                IsApprenticeshipDateAfter(date1End, date2End))
            {
                return true;
            }

            //In case of same month and year, if the dates span at least 1 whole month then they must straddle
            if (IsSameMonthYear(date1Start, date2Start) || IsSameMonthYear(date1End, date2End))
            {
                //if >= 1 month shared
                var startMonth = (date1Start.Year * 12) + date1Start.Month;
                var endMonth = (date1End.Year * 12) + date1End.Month;

                if ((endMonth - startMonth) >= 2)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsSameMonthYear(DateTime date1, DateTime date2)
        {
            return (date1.Month == date2.Month && date1.Year == date2.Year);
        }

    }
}
