namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlapRequests
{
    public class GetPendingOverlapRequestsQueryResult
    {
        public long? DraftApprenticeshipId { get; set; }
        public long? PreviousApprenticeshipId { get; set; }
        public DateTime? CreatedOn { get; set; }

        public GetPendingOverlapRequestsQueryResult(long draftApprenticeshipId, long previousApprenticeshipId, DateTime createdOn)
        {
            DraftApprenticeshipId = draftApprenticeshipId;
            PreviousApprenticeshipId = previousApprenticeshipId;
            CreatedOn = createdOn;
        }
    }
}
