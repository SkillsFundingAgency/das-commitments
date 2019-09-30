namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class FundingCapCourseSummary
    {
        public string CourseTitle { get; set; }
        public int ApprenticeshipCount { get; set; }

        public int Cap { get; set; }
        public decimal Cost { get; set; }

    }
}
