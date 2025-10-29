namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;

public class GetDraftApprenticeshipQuery(long cohortId, long draftApprenticeshipId)
    : IRequest<GetDraftApprenticeshipQueryResult>
{
    public long CohortId { get; } = cohortId;
    public long DraftApprenticeshipId { get; } = draftApprenticeshipId;
}