using System;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipEmailOverlapValidationRequest
    {
        public long? ApprenticeshipId { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime StartDate { get; set; }
        public string Email { get; set; }
    }
}
