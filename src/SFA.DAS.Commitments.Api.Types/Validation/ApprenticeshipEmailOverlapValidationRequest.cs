using System;

namespace SFA.DAS.Commitments.Api.Types.Validation
{
    public class ApprenticeshipEmailOverlapValidationRequest
    {
        public long? ApprenticeshipId { get; set; }

        public string Email { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
