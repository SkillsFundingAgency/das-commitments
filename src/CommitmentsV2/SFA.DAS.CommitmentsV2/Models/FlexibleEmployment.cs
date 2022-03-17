using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class FlexibleEmployment
    {
        public ApprenticeshipBase Apprenticeship { get; set; }
        public int EmploymentPrice { get; set; }
        public DateTime EmploymentEndDate { get; set; }
    }
}