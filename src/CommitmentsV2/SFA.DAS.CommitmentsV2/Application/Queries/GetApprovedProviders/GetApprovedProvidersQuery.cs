namespace SFA.DAS.CommitmentsV2.Application.Queries.GetApprovedProviders
{
    public class GetApprovedProvidersQuery : IRequest<GetApprovedProvidersQueryResult>
    {
        public long? AccountId { get; set; }

        public GetApprovedProvidersQuery(long? accountId)
        {
            AccountId = accountId;
        }
    }
}
