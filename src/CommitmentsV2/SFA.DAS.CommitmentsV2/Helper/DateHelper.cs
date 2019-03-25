using System;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Helper
{
    public class DateHelper
    {
        public static bool DateIsEmpty(int? day, int? month, int? year)
        {
            var dateModel = new DateModel { Day = day, Month = month, Year = year };
            return !dateModel.HasValue;
        }

        public static bool DateIsEmpty(int? month, int? year)
        {
            var dateModel = new MonthYearModel("") { Month = month, Year = year };
            return !dateModel.HasValue;
        }

        public static bool DateIsValid(int? day, int? month, int? year)
        {
            var dateModel = new DateModel { Day = day, Month = month, Year = year };
            return dateModel.IsValid;
        }

        public static bool DateIsValid(int? month, int? year)
        {
            var dateModel = new MonthYearModel("") { Month = month, Year = year };
            return dateModel.IsValid;
        }

        public static DateTime? ConvertToNullableDate(int? day, int? month, int? year)
        {
            var dateModel = new DateModel { Day = day, Month = month, Year = year };
            return dateModel.Date;
        }

        public static DateTime? ConvertToNullableDate(int? month, int? year)
        {
            var dateModel = new MonthYearModel("") { Month = month, Year = year };
            return dateModel.Date;
        }
    }
}