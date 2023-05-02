using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipPriorLearningSummary
{
    public class GetDraftApprenticeshipPriorLearningSummaryQuery : IRequest<GetDraftApprenticeshipPriorLearningSummaryQueryResult>
    {
        public GetDraftApprenticeshipPriorLearningSummaryQuery(long cohortId, long draftApprenticeshipId)
        {
            CohortId = cohortId;
            DraftApprenticeshipId = draftApprenticeshipId;
        }
        public long CohortId { get; }
        public long DraftApprenticeshipId { get; }
    }
}
