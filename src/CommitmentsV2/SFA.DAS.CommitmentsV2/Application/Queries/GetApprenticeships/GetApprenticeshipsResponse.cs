using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsResponse
    {
        public IEnumerable<ApprenticeshipDetails> Apprenticeships { get; set; }
        public int TotalApprenticeshipsFound { get; set; }
        public int TotalApprenticeshipsWithAlertsFound { get; set; }
        public int TotalApprenticeships { get; set; }
    }
}
