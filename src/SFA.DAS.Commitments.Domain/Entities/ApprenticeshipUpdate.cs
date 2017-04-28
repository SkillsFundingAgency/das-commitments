using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipUpdate
    {
        public long Id { get; set; }
        public long ApprenticeshipId { get; set; }
        public Originator Originator { get; set; }
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

        public bool HasChanges => !string.IsNullOrWhiteSpace(FirstName)
                                  || !string.IsNullOrWhiteSpace(LastName)
                                  || DateOfBirth.HasValue
                                  || TrainingType.HasValue
                                  || !string.IsNullOrWhiteSpace(TrainingCode)
                                  || !string.IsNullOrWhiteSpace(TrainingName)
                                  || Cost.HasValue
                                  || StartDate.HasValue
                                  || EndDate.HasValue;
    }
}
