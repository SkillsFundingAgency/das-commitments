using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain
{
    public class GetChangeOfProviderChainQueryResult
    {
        public IReadOnlyCollection<ChangeOfProviderLink> ChangeOfProviderChain { get; set; }

        public class ChangeOfProviderLink
        {
            public long ApprenticeshipId { get; set; }
            public long EmployerAccountId { get; set; }
            public string ProviderName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime? CreatedOn { get; set; }
            public long? ContinuationOfId { get; set; }
            public long? NewApprenticeshipId { get; set; }
        }
    }
}
