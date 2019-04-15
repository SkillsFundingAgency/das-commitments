using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.Services
{
    public static class ApprenticeshipExtensions
    {
        /// <summary>
        ///     Determines whether the supplied apprenticeship overlaps any apprenticeships in the supplied collection.
        /// </summary>
        /// <param name="apprenticeships">
        ///     The existing collection of apprenticeships for the same ULN. Only apprenticeships with the same
        ///     ULN as that of <see cref="apprenticeship"/>> will be considered. If <see cref="apprenticeships"/>
        ///     already contains an apprenticeship with the same id as <see cref="apprenticeship"/> then this
        ///     will also be ignored when determining overlaps.
        /// </param>
        /// <param name="apprenticeship">The apprenticeship that is to tested to see whether it overlaps the existing apprenticeships.</param>
        /// <returns>The overlap status</returns>
        public static OverlapStatus DetermineOverlap(this IEnumerable<Apprenticeship> apprenticeships, Apprenticeship apprenticeship)
        {
            string MessagePart(DateTime? time, string s) => apprenticeship.StartDate.HasValue ? $" has {s}" : " is missing {s}";

            if (!apprenticeship.StartDate.HasValue || !apprenticeship.EndDate.HasValue)
            {
                var msg =
                    "Cannot determine overlap for apprentice without both a start date and end date. "+
                    $"Apprentice {apprenticeship.Id} {MessagePart(apprenticeship.StartDate, "start date")} {MessagePart(apprenticeship.EndDate, "end date")}";

                throw new InvalidOperationException(msg);
            }

            return apprenticeships.DetermineOverlap(
                apprenticeship.Uln, 
                apprenticeship.StartDate.Value, 
                apprenticeship.EndDate.Value, 
                apprenticeship.Id);
        }

        /// <summary>
        ///     Determines whether the supplied draft apprenticeship overlaps any apprenticeships in the supplied collection.
        /// </summary>
        /// <param name="apprenticeships">
        ///     The existing collection of apprenticeships for the same ULN. Only apprenticeships with the same
        ///     ULN as that of <see cref="apprenticeship"/>> will be considered.
        /// </param>
        /// <param name="apprenticeship">The draft apprenticeship that is to tested to see whether it overlaps the existing apprenticeships.</param>
        /// <returns>The overlap status</returns>
        public static OverlapStatus DetermineOverlap(this IEnumerable<Apprenticeship> apprenticeships, DraftApprenticeshipDetails apprenticeship)
        {
            string MessagePart(DateTime? time, string s) => apprenticeship.StartDate.HasValue ? $" has {s}" : " is missing {s}";

            if (!apprenticeship.StartDate.HasValue || !apprenticeship.EndDate.HasValue)
            {
                var msg =
                    "Cannot determine overlap for apprentice without both a start date and end date. " +
                    $"Uln {apprenticeship.Uln} {MessagePart(apprenticeship.StartDate, "start date")} {MessagePart(apprenticeship.EndDate, "end date")}";

                throw new InvalidOperationException(msg);
            }

            return apprenticeships.DetermineOverlap(
                apprenticeship.Uln,
                apprenticeship.StartDate.Value,
                apprenticeship.EndDate.Value,
                null);
        }

        public static OverlapStatus DetermineOverlap(
            this IEnumerable<Apprenticeship> apprenticeships, 
            string uln, 
            DateTime startDate, 
            DateTime endDate, 
            long? apprenticeshipId)
        {
            if (string.IsNullOrWhiteSpace(uln))
            {
                return OverlapStatus.None;
            }

            var applicableApprenticeships = apprenticeships
                .Where(a => string.Equals(a.Uln, uln, StringComparison.OrdinalIgnoreCase) &&
                            (!apprenticeshipId.HasValue || a.Id != apprenticeshipId))
                .ToArray();

            if (applicableApprenticeships.Length == 0)
            {
                return OverlapStatus.None;
            }

            OverlapStatus result = OverlapStatus.None;

            foreach (var apprenticeship in applicableApprenticeships)
            {
                result = result | apprenticeship.DetermineOverlap(startDate, endDate);
            }

            return result;
        }

        public static DateTime GetEffectiveEndDate(this Apprenticeship apprenticeship)
        {
            //Get the appropriate dates for the apprenticeship
            if (apprenticeship.PaymentStatus == PaymentStatus.Withdrawn)
            {
                if (!apprenticeship.StopDate.HasValue)
                {
                    throw new InvalidOperationException($"Cannot determine duration of the apprenticeship because the payment status is {nameof(PaymentStatus.Withdrawn)} but the stop date has not been set.");
                }

                return apprenticeship.StopDate.Value;
            }

            if (!apprenticeship.EndDate.HasValue)
            {
                throw new InvalidOperationException($"Cannot determine duration of the apprenticeship because the stop date has not been set.");
            }

            return apprenticeship.EndDate.Value;
        }

        public static OverlapStatus DetermineOverlap(this Apprenticeship apprenticeship, DateTime startDate, DateTime endDate)
        {

            DateTime apprenticeshipEffectiveEndDate = apprenticeship.GetEffectiveEndDate();

            if (!apprenticeship.StartDate.HasValue)
            {
                throw new InvalidOperationException($"Cannot determine duration of the apprenticeship because the payment status is {nameof(PaymentStatus.Withdrawn)} but the start date has not been set.");
            }

            var apprenticeshipStartDate = apprenticeship.StartDate.Value.Date;

            //Stopped before or on start date (effectively deleted) should be ignored
            if (apprenticeship.PaymentStatus == PaymentStatus.Withdrawn && apprenticeshipStartDate == apprenticeshipEffectiveEndDate)
            {
                return OverlapStatus.None;
            }

            var overlapsStart = IsApprenticeshipDateBetween(startDate, apprenticeshipStartDate, apprenticeshipEffectiveEndDate);
            var overlapsEnd = IsApprenticeshipDateBetween(endDate, apprenticeshipStartDate, apprenticeshipEffectiveEndDate);

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
            if (IsApprenticeshipDateStraddle(startDate, endDate, apprenticeshipStartDate, apprenticeshipEffectiveEndDate))
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
