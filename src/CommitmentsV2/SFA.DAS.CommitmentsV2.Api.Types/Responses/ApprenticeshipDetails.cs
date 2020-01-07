using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class ApprenticeshipDetails
    {
        public long Id { get; set; }
        public string ApprenticeFirstName { get; set; }
        public string ApprenticeLastName { get; set; }
        public string Uln { get; set; }
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDateTime { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public IEnumerable<string> Alerts { get; set; }
    }

}
