using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class FlexibleEmployment
    {
        public FlexibleEmployment()
        {
        }

        public FlexibleEmployment(long apprenticeshipId, int employmentPrice, DateTime employmentEndDate)
        {
            EmploymentPrice = employmentPrice;
            EmploymentEndDate = employmentEndDate;
            ApprenticeshipId = apprenticeshipId;
        }
        public Apprenticeship Apprenticeship { get; set; }
        public long ApprenticeshipId { get; set; }
        public int EmploymentPrice { get; set; }
        public DateTime EmploymentEndDate { get; set; }
    }
}