using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Common.Domain.Types;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipStatusSummary
{
    public class GetApprenticeshipStatusSummaryQueryResults
    {
        public IEnumerable<GetApprenticeshipStatusSummaryQueryResult> GetApprenticeshipStatusSummaryQueryResult { get; set; }
    }

    public class GetApprenticeshipStatusSummaryQueryResult
    {
        public string LegalEntityIdentifier { get; set; }
        public OrganisationType LegalEntityOrganisationType { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public int Count { get; set; }
        public int PendingApprovalCount { get; set; }
        public int ActiveCount { get; set; }
        public int PausedCount { get; set; }
        public int WithdrawnCount { get; set; }
        public int CompletedCount { get; set; }
    }    
}
