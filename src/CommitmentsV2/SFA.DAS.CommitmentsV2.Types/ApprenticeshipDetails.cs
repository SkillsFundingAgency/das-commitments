using System;

namespace SFA.DAS.CommitmentsV2.Types
{
    public class ApprenticeshipDetails
    {
        public string ApprenticeName { get; set; }
        public string Uln { get; set; }
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public DateTime PlannedStartDate { get; set; }
        public DateTime PlannedEndDateTime { get; set; }
        public string Status { get; set; }
        public string Alerts { get; set; }

    }
}
