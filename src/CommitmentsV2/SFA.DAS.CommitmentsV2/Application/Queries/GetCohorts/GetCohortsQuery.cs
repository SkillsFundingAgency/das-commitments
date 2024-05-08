namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts
{
    public class GetCohortsQuery : IRequest<GetCohortsResult>
    {
        public long? AccountId { get; }
        public long? ProviderId { get; }

        public GetCohortsQuery(long? accountId, long? providerId)
        {
            AccountId = accountId;
            ProviderId = providerId;
        }
    }
}