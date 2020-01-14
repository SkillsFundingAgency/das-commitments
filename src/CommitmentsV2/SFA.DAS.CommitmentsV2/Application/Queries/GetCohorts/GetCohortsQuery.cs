using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts
{
    public class GetCohortsQuery : IRequest<GetCohortsResult>
    {
        public long? AccountId { get; }

        public GetCohortsQuery(long? accountId)
        {
            AccountId = accountId;
        }
    }
}