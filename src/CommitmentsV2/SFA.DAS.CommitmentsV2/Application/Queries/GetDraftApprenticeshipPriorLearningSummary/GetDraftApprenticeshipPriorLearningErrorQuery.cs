using MediatR;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningError;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningError
{
    public class GetDraftApprenticeshipPriorLearningErrorQuery : IRequest<GetDraftApprenticeshipPriorLearningErrorQueryResult>
    {
        public GetDraftApprenticeshipPriorLearningErrorQuery(long cohortId)
        {
            CohortId = cohortId;
        }
        public long CohortId { get; }
    }
}
