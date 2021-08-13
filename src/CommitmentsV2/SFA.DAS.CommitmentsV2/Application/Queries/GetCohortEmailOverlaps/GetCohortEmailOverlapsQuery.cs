using MediatR;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohortSummary;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortEmailOverlaps
{
    public class GetCohortEmailOverlapsQuery : IRequest<GetCohortEmailOverlapsQueryResult>
    {
        public long CohortId { get; }

        public GetCohortEmailOverlapsQuery(long cohortId)
        {
            CohortId = cohortId;
        }
    }
}
