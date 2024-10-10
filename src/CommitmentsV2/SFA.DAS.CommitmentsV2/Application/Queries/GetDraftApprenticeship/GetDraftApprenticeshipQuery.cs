namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

public class GetDraftApprenticeshipQuery : IRequest<GetDraftApprenticeshipQueryResult>
{
    public GetDraftApprenticeshipQuery(long cohortId, long draftApprenticeshipId)
    {
        CohortId = cohortId;
        DraftApprenticeshipId = draftApprenticeshipId;
    }
    public long CohortId { get; }
    public long DraftApprenticeshipId { get; }
}