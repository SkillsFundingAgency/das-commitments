using MediatR;

namespace SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportCohortSummary
{
    public class GetSupportCohortSummaryQuery : IRequest<GetSupportCohortSummaryQueryResult>
    {
        public GetSupportCohortSummaryQuery(long cohortId, long accountId)
        {
            AccountId = accountId;
            CohortId = cohortId;
        }

        public long CohortId { get; }
        public long AccountId { get; }
    }
}