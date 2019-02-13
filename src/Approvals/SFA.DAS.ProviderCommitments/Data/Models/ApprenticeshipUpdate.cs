using System;
using System.Collections.Generic;

namespace SFA.DAS.ProviderCommitments.Data.Models
{
    public partial class ApprenticeshipUpdate
    {
        public ApprenticeshipUpdate()
        {
            DataLockStatus = new HashSet<DataLockStatus>();
        }

        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public byte Originator { get; set; }
        public byte Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? CreatedOn { get; set; }
        public byte? UpdateOrigin { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }

        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
    }
}
