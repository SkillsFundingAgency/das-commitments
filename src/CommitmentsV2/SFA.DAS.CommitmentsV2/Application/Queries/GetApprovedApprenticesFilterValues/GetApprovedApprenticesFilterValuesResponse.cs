using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprenticesFilterValues
{
    public class GetApprovedApprenticesFilterValuesResponse
    {
        public IEnumerable<string> EmployerNames { get; set; }
    }
}
