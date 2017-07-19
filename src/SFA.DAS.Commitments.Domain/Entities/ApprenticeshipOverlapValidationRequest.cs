using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipOverlapValidationRequest
    {
        public long? ApprenticeshipId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string Uln { get; set; }
    }
}