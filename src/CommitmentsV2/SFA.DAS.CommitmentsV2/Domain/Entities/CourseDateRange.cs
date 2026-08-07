using SFA.DAS.CommitmentsV2.Domain.Extensions;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class CourseDateRange
{
    public DateTime From { get; }
    public DateTime To { get; }
    public bool HasWithdrawn { get; }

    public CourseDateRange(DateTime from, DateTime to, bool hasWithdrawn = false)
    {
        From = from;
        To = to;
        HasWithdrawn = hasWithdrawn;
    }

    public bool IsZeroDays => From.Equals(To);

    public bool IsSingleMonth => From.IsSameMonthAndYear(To);

    public bool Contains(DateTime value)
    {
        return From.IsBeforeMonth(value) && To.IsAfterMonth(value);
    }

    public OverlapStatus DetermineOverlap(CourseDateRange range)
    {
        if (CourseIsCompletelyCancelled() || range.CourseIsCompletelyCancelled())
        {
            return OverlapStatus.None;
        }

        var overlapsStart = Contains(range.From);
        var overlapsEnd = Contains(range.To);

        //Contained
        if (overlapsStart && overlapsEnd)
        {
            return OverlapStatus.DateWithin;
        }

        //Overlap start date
        if (overlapsStart)
        {
            return OverlapStatus.OverlappingStartDate;
        }

        //Overlap end date
        if (overlapsEnd)
        {
            return OverlapStatus.OverlappingEndDate;
        }

        //Clear straddle
        if (range.From.IsBeforeMonth(From) && range.To.IsAfterMonth(To))
        {
            return OverlapStatus.DateEmbrace;
        }

        //Range is within a single month without a clear straddle
        if (range.IsSingleMonth)
        {
            return OverlapStatus.None;
        }

        //Straddle sharing a start or end date
        if (range.From.IsSameMonthAndYear(From) || range.To.IsSameMonthAndYear(To))
        {
            return OverlapStatus.DateEmbrace;
        }

        return OverlapStatus.None;
    }

    public bool CourseIsCompletelyCancelled()
    {
        return IsZeroDays && HasWithdrawn;
    }
}