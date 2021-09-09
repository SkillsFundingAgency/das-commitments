using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Types.Validation
{
    public class ApprenticeshipEmailOverlapValidationResult
    {
        public ApprenticeshipEmailOverlapValidationRequest Self { get; set; }

        public IEnumerable<OverlappingApprenticeship> OverlappingApprenticeships { get; set; }
    }
}