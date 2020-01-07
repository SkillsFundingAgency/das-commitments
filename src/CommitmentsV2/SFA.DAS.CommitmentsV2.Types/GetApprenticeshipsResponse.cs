using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Types
{
    public class GetApprenticeshipsResponse
    {
        public IEnumerable<ApprenticeshipDetails> Apprenticeships { get; set; }
        public int TotalApprenticeshipsFound { get; set; }
    }
}
