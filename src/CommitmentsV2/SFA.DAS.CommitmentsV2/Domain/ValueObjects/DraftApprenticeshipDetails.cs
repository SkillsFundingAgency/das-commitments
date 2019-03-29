using System;

namespace SFA.DAS.CommitmentsV2.Domain.ValueObjects
{
    public class DraftApprenticeshipDetails
    {
        //todo: make this a real value object
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Uln { get; set; }
        public int? TrainingType { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
        public decimal? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string EmployerRef { get; set; }
        public string ProviderRef { get; set; }
        public Guid? ReservationId { get; set; }
    }
}
