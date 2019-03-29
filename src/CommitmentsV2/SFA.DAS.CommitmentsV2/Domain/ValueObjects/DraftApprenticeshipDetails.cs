using System;

namespace SFA.DAS.CommitmentsV2.Domain.ValueObjects
{
    public class DraftApprenticeshipDetails
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public int? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Reference { get; set; }
        public Guid? ReservationId { get; set; }
    }
}
