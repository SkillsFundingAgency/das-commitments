namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class FundingCapCourseSummary
{
    public string CourseTitle { get; set; }
    public int ApprenticeshipCount { get; set; }
    public int ActualCap { get; set; }
    public decimal CappedCost { get; set; }
}