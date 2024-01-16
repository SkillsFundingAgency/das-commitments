using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval
{
    public class GetEmployerCohortsReadyForApprovalQueryResults
    {
        public IEnumerable<GetEmployerCohortsReadyForApprovalQueryResult> GetEmployerCohortsReadyForApprovalQueryResult { get; set; }
    }

    public class GetEmployerCohortsReadyForApprovalQueryResult
    {
        public long CohortId { get; set; }
        public string CohortReference { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public long? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public long? TransferSenderId { get; set; }
        public string TransferSenderName { get; set; }
    }
}