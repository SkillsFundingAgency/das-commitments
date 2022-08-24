using System;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlapRequests
{
    public class GetOverlapRequestsQueryResult
    {
        public long? DraftApprenticeshipId { get; set; }
        public long? PreviousApprenticeshipId { get; set; }
        public DateTime? CreatedOn { get; set; }

        public GetOverlapRequestsQueryResult(long draftApprenticeshipId, long previousApprenticeshipId, DateTime createdOn)
        {
            DraftApprenticeshipId = draftApprenticeshipId;
            PreviousApprenticeshipId = previousApprenticeshipId;
            CreatedOn = createdOn;
        }
    }
}
