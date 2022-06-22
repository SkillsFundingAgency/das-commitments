using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship
{
    public class GetSupportApprenticeshipQueryResult
    {
        public List<SupportApprenticeshipDetails> Apprenticeships { get; set; } = new List<SupportApprenticeshipDetails>();
    }
}