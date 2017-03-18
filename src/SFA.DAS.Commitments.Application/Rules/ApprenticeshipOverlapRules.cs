using System;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.Commitments.Api.Types.Validation.Types;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Rules
{
    public class ApprenticeshipOverlapRules : IApprenticeshipOverlapRules
    {
        public ValidationFailReason DetermineOverlap(ApprenticeshipOverlapValidationRequest request, ApprenticeshipResult apprenticeship)
        {
            if (!request.Uln.Equals(apprenticeship.Uln, StringComparison.InvariantCultureIgnoreCase))
            {
                return ValidationFailReason.None;
            }

            if (request.ApprenticeshipId.HasValue && request.ApprenticeshipId.Value == apprenticeship.Id)
            {
                return ValidationFailReason.None;
            }

            //Get the appropriate dates for the apprenticeship
            //Additional logic to select other dates based on status can go here
            var apprenticeshipStartDate = apprenticeship.StartDate;
            var apprenticeshipEndDate = apprenticeship.EndDate;

            var overlapsStart = IsApprenticeshipDateBetween(request.StartDate, apprenticeshipStartDate, apprenticeshipEndDate);
            var overlapsEnd = IsApprenticeshipDateBetween(request.EndDate, apprenticeshipStartDate, apprenticeshipEndDate);

            //Contained
            if (overlapsStart && overlapsEnd)
            {
                return ValidationFailReason.DateWithin;
            }

            //Overlap start date
            if (overlapsStart)
            {
                return ValidationFailReason.OverlappingStartDate;
            }

            //Overlap end date
            if (overlapsEnd)
            {
                return ValidationFailReason.OverlappingEndDate;
            }

            //Straddle
            if (IsApprenticeshipDateStraddle(request.StartDate, request.EndDate, apprenticeshipStartDate, apprenticeshipEndDate))
            {
                return ValidationFailReason.DateEmbrace;
            }

            return ValidationFailReason.None;
        }

        private static bool IsApprenticeshipDateBetween(DateTime dateToCheck, DateTime dateFrom, DateTime dateTo)
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

        private static bool IsApprenticeshipDateStraddle(DateTime date1Start, DateTime date1End, DateTime date2Start, DateTime date2End)
        {
            //straightforward case - clear straddle
            if (IsApprenticeshipDateBefore(date1Start, date2Start) && IsApprenticeshipDateAfter(date1End, date2End))
            {
                return true;
            }

            //Case where active apprenticeship is single-month, cannot overlap
            if (IsSameMonthYear(date2Start, date2End))
            {
                return false;
            }

            //Case where timespans are identical
            if (IsSameMonthYear(date1Start, date2Start) && IsSameMonthYear(date1End, date2End))
            {
                //Then single month apprenticeships do not overlap
                if (IsSameMonthYear(date1Start, date1End))
                {
                    return false;
                }

                return true;
            }

            //If the apprenticeships share a start date
            if (IsSameMonthYear(date1Start, date2Start))
            {
                //The if is a single month then do not overlap
                if (IsSameMonthYear(date1Start, date1End))
                {
                    return false;
                }

                return true;
            }

            //If they share an end date
            if (IsSameMonthYear(date1End, date2End))
            {
                //Then if is a single month then do not overlap
                if (IsSameMonthYear(date1Start, date1End))
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        private static bool IsSameMonthYear(DateTime date1, DateTime date2)
        {
            return (date1.Month == date2.Month && date1.Year == date2.Year);
        }

    }
}
