using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedApprentices
{
    public class GetApprovedApprenticesResponse
    {
        public IEnumerable<ApprenticeshipDetails> Apprenticeships { get; set; }    
    }
}
