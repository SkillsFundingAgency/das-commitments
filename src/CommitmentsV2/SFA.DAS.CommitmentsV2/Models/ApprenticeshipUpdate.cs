using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Models.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipUpdate : ITrackableEntity
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public Originator Originator { get; set; }
        public ApprenticeshipUpdateStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ProgrammeType? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? CreatedOn { get; set; }
        public ApprenticeshipUpdateOrigin? UpdateOrigin { get; set; }
        public DateTime? EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }
        public virtual ICollection<DataLockStatus> DataLockStatus { get; set; }
        
        
        public virtual ApprenticeshipBase Apprenticeship { get; set; }
    }
}
