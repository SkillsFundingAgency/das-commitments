using System;

namespace SFA.DAS.CommitmentsV2.Types
{
    public class CohortSummary
    {
        public long AccountId { get; set; }
        public string LegalEntityName { get; set; }
        public long ProviderId { get; set; }
        public string ProviderName { get; set; }
        public long CohortId { get; set; }
        public int NumberOfDraftApprentices { get; set; }
        public Message LatestMessageFromProvider { get; set; }
        public Message LatestMessageFromEmployer { get; set; }
        public bool IsDraft { get; set; }
        public Party WithParty { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? TransferSenderId { get; set; }
        public string TransferSenderName { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public bool IsLinkedToChangeOfPartyRequest { get; set; }
        public CommitmentStatus CommitmentStatus { get; set; }
        public int? PledgeApplicationId { get; set; }
    }
}