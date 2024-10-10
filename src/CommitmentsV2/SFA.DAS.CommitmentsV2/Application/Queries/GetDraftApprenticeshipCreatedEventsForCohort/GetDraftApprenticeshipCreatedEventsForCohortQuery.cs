namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeshipCreatedEventsForCohort;

public class GetDraftApprenticeshipCreatedEventsForCohortQuery : IRequest<GetDraftApprenticeshipCreatedEventsForCohortQueryResult>
{
    public long ProviderId { get; }
    public long CohortId { get; }
    public uint NumberOfApprentices { get; }
    public DateTime UploadedOn { get; }

    public GetDraftApprenticeshipCreatedEventsForCohortQuery(long providerId, long cohortId, uint numberOfApprentices, DateTime uploadedOn)
    {
        ProviderId = providerId;
        CohortId = cohortId;
        NumberOfApprentices = numberOfApprentices;
        UploadedOn = uploadedOn;
    }
}