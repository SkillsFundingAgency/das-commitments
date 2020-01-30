using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprenticeshipsFilterValuesResponse
    {
        public IEnumerable<string> EmployerNames { get; set; }
        public IEnumerable<string> CourseNames { get; set; }
        public IEnumerable<PaymentStatus> Statuses { get; set; }
        public IEnumerable<DateTime> StartDates { get; set; }
        public IEnumerable<DateTime> EndDates { get; set; }
    }
}
