using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprentice
{
    public class GetDraftApprenticeRequest : IRequest<GetDraftApprenticeResponse>
    {
        public GetDraftApprenticeRequest(long cohortId, long draftApprenticeshipId)
        {
            CohortId = cohortId;
            DraftApprenticeshipId = draftApprenticeshipId;
        }
        public long CohortId { get; }
        public long DraftApprenticeshipId { get; }
    }
}
