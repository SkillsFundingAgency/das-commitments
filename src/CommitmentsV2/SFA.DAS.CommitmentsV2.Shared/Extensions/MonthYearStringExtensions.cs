using SFA.DAS.CommitmentsV2.Shared.Models;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions;

public static class MonthYearStringExtensions
{
    public static bool IsValidMonthYear(this string monthYear)
    {
        try
        {
            var dateModel = new MonthYearModel(monthYear);
            return dateModel.IsValid;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}