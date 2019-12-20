using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships
{
    public class GetApprenticeshipsResponse
    {
        public IEnumerable<ApprenticeshipDetails> Apprenticeships { get; set; }    
    }
}
