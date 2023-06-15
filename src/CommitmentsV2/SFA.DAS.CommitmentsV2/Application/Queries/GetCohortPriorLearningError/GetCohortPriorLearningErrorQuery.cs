using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCohortPriorLearningError
{
    public class GetCohortPriorLearningErrorQuery : IRequest<GetCohortPriorLearningErrorQueryResult>
    {
        public GetCohortPriorLearningErrorQuery(long cohortId)
        {
            CohortId = cohortId;
        }
        public long CohortId { get; }
    }
}
