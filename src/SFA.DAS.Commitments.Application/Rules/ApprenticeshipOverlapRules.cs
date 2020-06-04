using System;

using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Entities.Validation;

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
            var apprenticeshipStartDate = apprenticeship.StartDate;
            var apprenticeshipEndDate = CalculateOverlapApprenticeshipEndDate(apprenticeship);

            //Stopped before or on start date (effectively deleted) should be ignored
            if (apprenticeship.PaymentStatus == PaymentStatus.Withdrawn &&
                apprenticeshipStartDate.Date.Equals(apprenticeship.StopDate.Value.Date))
            {
                return ValidationFailReason.None;
            }

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

        /// <summary>
        /// Calculates what date should be used as the overlap end date for an apprenticeship when validating start date / end date overlaps.
        /// </summary>
        private DateTime CalculateOverlapApprenticeshipEndDate(ApprenticeshipResult apprenticeship)
        {
            switch (apprenticeship.PaymentStatus)
            {
                case PaymentStatus.Withdrawn:
                    return apprenticeship.StopDate.Value;

                case PaymentStatus.Completed:
                    if (apprenticeship.CompletionDate.Value <= apprenticeship.EndDate)
                    {
                        return apprenticeship.CompletionDate.Value;
                    }
                    return apprenticeship.EndDate;

                default:
                    return apprenticeship.EndDate;
            }
        }
    }
}
