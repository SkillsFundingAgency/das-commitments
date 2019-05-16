namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public sealed class GetCohortResponse
    {
        public long CohortId { get; set; }
        public string AccountLegalEntityPublicHashedId { get; set; }
        public string LegalEntityName { get; set; }
    }
}