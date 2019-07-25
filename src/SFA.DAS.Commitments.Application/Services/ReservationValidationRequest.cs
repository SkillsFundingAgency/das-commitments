using System;

namespace SFA.DAS.Commitments.Application.Services
{
    public class ReservationValidationServiceRequest
    {
        public long CommitmentId { get; set; }
        public long ApprenticeshipId { get; set; }
        public long? ProviderId { get; set; }
        public long AccountId { get; set; }
        public Guid? ReservationId { get; set; }
        public DateTime? StartDate { get; set; }
        public string TrainingCode { get; set; }
    }
}