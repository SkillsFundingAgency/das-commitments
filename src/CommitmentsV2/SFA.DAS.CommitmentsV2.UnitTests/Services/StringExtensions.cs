using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    internal static class StringExtensions
    {
        private static readonly string[] Months = new[]
            {"JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"};

        /// <summary>
        ///     Convert a string in the format "Apr-19 to Jun-2019, Aug-20 or Jan-2021" into an enumeration of start-end period pairs.
        /// </summary>
        public static IEnumerable<(DateTime StartDate, DateTime EndDate)> StartEndPeriods(this string s)
        {
            return s.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(datePair => StartEndPeriod(datePair.Trim()));
        }

        /// <summary>
        ///     Convert a string in the format "Apr-19 to Jun-2019" in to a pair of dates representing start and end.
        /// </summary>
        public static (DateTime StartDate, DateTime EndDate) StartEndPeriod(this string s)
        {
            // We don't actually care what comes in the middle - we'll use everything up to the first space as date 1 and 
            // everything after the last space as date 2.

            var firstSpace = s.IndexOf(' ');
            var lastSpace = s.LastIndexOf(' ');

            if (firstSpace == -1 || lastSpace == -1)
            {
                throw new InvalidOperationException($"The value {s} should be in the format \"mmm-yy to mmm-yy\" (or yy may be yyyy)");
            }

            var date1 = s.Substring(0, firstSpace);
            var date2 = s.Substring(lastSpace+1);

            return (date1.MonthYear(), date2.MonthYear());
        }

        /// <summary>
        ///     Convert a string in the format Apr-19 or Apr-2019, Jun-20 or Jun-2020 etc
        ///     in to a date of the 1st of the month.
        /// </summary>
        public static DateTime MonthYear(this string s)
        {
            var parts = s.Split('-').ToArray();
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"The string {s} should have two parts separated by a dash: mmm-yy or mmm-yyyy");
            }

            if (parts[0].Length < 3)
            {
                throw new InvalidOperationException($"The string {s} should have a month part with at least three letters: mmm-yy or mmm-yyyy");
            }

            if (parts[1].Length != 2 && parts[1].Length != 4)
            {
                throw new InvalidOperationException($"The string {s} should have a year that is either 2 or 4 digits: mmm-yy or mmm-yyyy");
            }

            var month = parts[0].Substring(0, 3).ToUpperInvariant();

            var monthIdx = Months.IndexOf(month);

            if (monthIdx < 0)
            {
                throw new InvalidOperationException($"The month part in {s} ({month} is not a recognised month");
            }

            if (!int.TryParse(parts[1], out int year))
            {
                throw new InvalidOperationException($"The year part in {s} ({parts[1]} is not a valid year - should be yy or yyyy");
            }

            return new DateTime(year, monthIdx + 1, 1);
        }
    }
}