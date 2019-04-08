using System;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime FirstOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, 1);
        }

        public static int Age(this DateTime dateOfBirth, DateTime asAt)
        {
            var age = asAt.Year - dateOfBirth.Year;
            if ((dateOfBirth.Month > asAt.Month) || (dateOfBirth.Month == asAt.Month && dateOfBirth.Day > asAt.Day)) age--;

            return age;
        }
    }
}
