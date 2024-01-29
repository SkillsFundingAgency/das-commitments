using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship
{
    public class GetSupportApprenticeshipQueryResult
    {
        public List<SupportApprenticeshipDetails> Apprenticeships { get; set; } = new List<SupportApprenticeshipDetails>();
    }
}