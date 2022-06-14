using System;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToGdsFormatWithoutDay(this DateTime date)
        {
            return date.ToString("MMM yyyy");
        }

        public static string ToGdsFormatWithoutDay(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("MMM yyyy") : string.Empty;
        }

        public static string ToGdsFormatWithSlashSeperator(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("MM/yy") : string.Empty;
        }

        public static string ToGdsFormatWithSpaceSeperator(this DateTime? date)
        {
            return date.HasValue ? date.Value.ToString("dd MMMM yyyy") : string.Empty;
        }

        public static string ToGdsFormatWithSlashSeperator(this DateTime date)
        {
            return date.ToString("MM/yy");
        }

        public static string ToGdsFormatWithSpaceSeperator(this DateTime date)
        {
            return date.ToString("dd MMMM yyyy");
        }
    }
}