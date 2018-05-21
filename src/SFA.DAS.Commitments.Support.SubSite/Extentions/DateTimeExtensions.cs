using System;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime GetLastDayOfMonthDate(this DateTime date)
        {
            var lastDay = DateTime.DaysInMonth(date.Year, date.Month);

            return new DateTime(date.Year, date.Month, lastDay);
        }

        public static string ToGdsFormat(this DateTime date)
        {
            return date.ToString("d MMM yyyy");
        }

        public static string ToGdsFormatWithoutDay(this DateTime date)
        {
            return date.ToString("MMM yyyy");
        }

        public static string ToGdsFormatShortMonthWithoutDay(this DateTime date)
        {
            return date.ToString("MM yyyy");
        }

        public static string ToGdsFormatLongMonthNameWithoutDay(this DateTime date)
        {
            return date.ToString("MMMM yyyy");
        }

        public static string ToGdsFormatWithSlashSeperator(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("MM/yy") : string.Empty;
        }

        public static string ToGdsFormatWithSpaceSeperator(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd MMMM yyyy") : string.Empty;
        }
    }
}