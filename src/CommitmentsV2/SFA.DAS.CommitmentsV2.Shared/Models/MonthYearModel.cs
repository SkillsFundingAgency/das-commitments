namespace SFA.DAS.CommitmentsV2.Shared.Models;

public class MonthYearModel : DateModel
{
    public MonthYearModel(string monthYear)
    {
        SourceValue = monthYear;
        SetFromMonthYear(monthYear);
    }

    [JsonIgnore]
    public override int? Day
    {
        get => 1; // always use first day of month
        set => throw new InvalidOperationException("Cannot set the day on a month-year value");
    }

    public string MonthYear => $"{Month:D2}{Year:D4}";
    public string SourceValue { get; }

    public override bool HasValue => Month.HasValue || Year.HasValue;

    private void SetFromMonthYear(string monthYear)
    {
        var mmyyyyLength = "MMYYYY".Length;
        var myyyyLength = "MYYYY".Length;
        if (string.IsNullOrWhiteSpace(monthYear))
        {
            return;
        }

        if (monthYear.Length < myyyyLength ||
            monthYear.Length > mmyyyyLength ||
            !int.TryParse(monthYear, out _))
        {
            throw new ArgumentException("The month and year must be in the format mmyyyy or myyyy", nameof(monthYear));
        }

        var monthLength = monthYear.Length == myyyyLength ? 1 : 2;

        Year = int.Parse(monthYear.Substring(monthLength));
        Month = int.Parse(monthYear.Substring(0, monthLength));
            
        if (!IsValid)
        {
            throw new ArgumentException($"Either the month year {monthYear} is not valid or the day {Day} is not valid for this month.");
        }
    }
}