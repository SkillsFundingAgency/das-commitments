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
        public string LastMessageFromProvider { get; set; }
        public string LastMessageFromEmployer { get; set; }
        public bool IsDraft { get; set; }
        public Party WithParty { get; set; }
    }
}