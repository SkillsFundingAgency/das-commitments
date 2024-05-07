namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary
{
    public class GetAccountSummaryQuery : IRequest<GetAccountSummaryQueryResult>
    {
        public long AccountId { get; set; }
    }
}
