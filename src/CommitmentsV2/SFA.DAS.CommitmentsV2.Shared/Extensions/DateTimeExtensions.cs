using System;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ToGdsFormat(this DateTime value)
        {
            return $"{value:d MMM yyyy}";
        }
        public static string ToGdsFormatLongMonth(this DateTime value)
        {
            return $"{value:d MMMM yyyy}";
        }
        public static string ToGdsFormatWithoutDay(this DateTime value)
        {
            return $"{value:MMM yyyy}";
        }
        public static string ToGdsFormatLongMonthWithoutDay(this DateTime value)
        {
            return $"{value:MMMM yyyy}";
        }

        public static string ToGdsFormatShortMonthWithoutDay(this DateTime value)
        {
            return $"{value:MM yyyy}";
        }
    }
}
