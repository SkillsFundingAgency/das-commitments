using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Extensions;

public static class DateTimeExtensions
{
    public static DateTime FirstOfMonth(this DateTime value)
    {
        return new DateTime(value.Year, value.Month, 1);
    }

    public static CourseDateRange To(this DateTime self, DateTime to, bool isWithdrawn = false)
    {
        return new CourseDateRange(self.Date, to, isWithdrawn);
    }

    public static bool IsBeforeMonth(this DateTime self, DateTime value)
    {
        if (self.Year < value.Year) return true;
        if (self.Year > value.Year) return false;
        return self.Month < value.Month;
    }

    public static bool IsAfterMonth(this DateTime self, DateTime value)
    {
        return value.IsBeforeMonth(self);
    }

    public static bool IsSameMonthAndYear(this DateTime self, DateTime value)
    {
        return self.Month == value.Month && self.Year == value.Year;
    }
}