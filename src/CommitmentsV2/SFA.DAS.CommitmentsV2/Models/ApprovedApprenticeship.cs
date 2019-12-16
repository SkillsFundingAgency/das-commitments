using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprovedApprenticeship : Apprenticeship
    {
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        public virtual ICollection<PriceHistory> PriceHistory { get; set; }

        public int? PaymentOrder { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public bool HasHadDataLockSuccess { get; set; }
        public byte? PendingUpdateOriginator { get; set; }


        public ApprovedApprenticeship()
        {
            DataLockStatus = new List<DataLockStatus>();
            PriceHistory = new List<PriceHistory>();
        }

        public static implicit operator ApprenticeshipDetails(ApprovedApprenticeship source)
        {
            return new ApprenticeshipDetails
            {
                ApprenticeFirstName = source.FirstName,
                ApprenticeLastName = source.LastName,
                CourseName = source.CourseName,
                EmployerName = source.Cohort.LegalEntityName,
                PlannedStartDate = source.StartDate.GetValueOrDefault(),
                PlannedEndDateTime = source.EndDate.GetValueOrDefault(),
                PaymentStatus = source.PaymentStatus,
                Uln = source.Uln,
                DataLockStatus = source.DataLockStatus.Select(status => status.Status)
            };
        }
    }
}