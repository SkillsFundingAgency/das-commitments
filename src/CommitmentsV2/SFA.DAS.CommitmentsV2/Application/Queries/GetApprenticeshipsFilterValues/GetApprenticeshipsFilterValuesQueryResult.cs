using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsFilterValues
{
    public class GetApprenticeshipsFilterValuesQueryResult
    {
        public IEnumerable<string> EmployerNames { get; set; }
        public IEnumerable<string> CourseNames { get; set; }
        public IEnumerable<string> Statuses { get; set; }
        public IEnumerable<string> PlannedStartDates { get; set; }
        public IEnumerable<string> PlannedEndDates { get; set; }
    }
}
