using System;

namespace SFA.DAS.Commitments.Application.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        //public static DateTime EndOfMonth(this DateTime value)
        //{
        //    return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        //}
    }
}
