using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class ApprenticeshipUpdateDetails
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public Originator Originator { get; set; }
        public ApprenticeshipUpdateStatus Status { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public TrainingType? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedOn { get; set; }
        public UpdateOrigin UpdateOrigin { get; set; }
        public DateTime EffectiveFromDate { get; set; }
        public DateTime? EffectiveToDate { get; set; }

        public bool HasChanges => !string.IsNullOrWhiteSpace(FirstName)
                                  || !string.IsNullOrWhiteSpace(LastName)
                                  || DateOfBirth.HasValue
                                  || TrainingType.HasValue
                                  || !string.IsNullOrWhiteSpace(TrainingCode)
                                  || !string.IsNullOrWhiteSpace(TrainingName)
                                  || Cost.HasValue
                                  || StartDate.HasValue
                                  || EndDate.HasValue;


        public string ULN { get; set; }
        public string ProviderRef { get; set; }
        public string EmployerRef { get; set; }
    }
}
