using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfEmployerChain
{
    public class GetChangeOfEmployerChainQueryResult
    {
        public IReadOnlyCollection<ChangeOfEmployerLink> ChangeOfEmployerChain { get; set; }

        public class ChangeOfEmployerLink
        {
            public long ApprenticeshipId { get; set; }
            public long Ukprn { get; set; }
            public bool EmployerIsDeleted { get; set; }
            public string EmployerName { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? StopDate { get; set; }
            public DateTime? CreatedOn { get; set; }
            public long? ContinuationOfId { get; set; }
            public long? NewApprenticeshipId { get; set; }
        }
    }
}
