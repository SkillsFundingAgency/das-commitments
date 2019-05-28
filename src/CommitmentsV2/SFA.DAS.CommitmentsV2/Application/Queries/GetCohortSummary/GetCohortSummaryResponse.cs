namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary
{
    public class GetCohortSummaryResponse
    {
        public long CohortId { get; set; }
        public string LegalEntityName { get; set; }
        public long? ProviderId { get; set; }
        public long AccountId { get; set; }
        public long? TransferSenderAccountId { get; set; }

    }
}
