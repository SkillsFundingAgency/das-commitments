using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class FlexibleEmployment
    {
        public ApprenticeshipBase Apprenticeship { get; private set; }
        public int EmploymentPrice { get; set; }
        public DateTime EmploymentEndDate { get; set; }
    }
}