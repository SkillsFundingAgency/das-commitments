namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryResponse
    {
        public long AccountId { get; set; }
        public bool HasCohorts { get; set; }
        public bool HasApprenticeships { get; set; }
    }
}
