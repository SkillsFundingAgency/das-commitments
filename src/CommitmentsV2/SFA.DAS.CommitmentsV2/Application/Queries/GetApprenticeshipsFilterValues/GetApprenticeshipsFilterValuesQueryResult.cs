namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryResult
    {
        public IEnumerable<string> EmployerNames { get; set; }
        public IEnumerable<string> ProviderNames { get; set; }
        public IEnumerable<string> CourseNames { get; set; }
        public IEnumerable<DateTime> StartDates { get; set; }
        public IEnumerable<DateTime> EndDates { get; set; }
        public IEnumerable<string> Sectors { get; set; }
    }
}
