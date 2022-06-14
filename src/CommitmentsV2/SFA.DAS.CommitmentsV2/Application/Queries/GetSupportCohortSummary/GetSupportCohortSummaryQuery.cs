using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortApprenticeships
{
    public class GetSupportCohortSummaryQuery : IRequest<GetSupportCohortSummaryQueryResult>
    {
        public GetSupportCohortSummaryQuery(long cohortId)
        {
            CohortId = cohortId;
        }

        public long CohortId { get; set; }
    }
}