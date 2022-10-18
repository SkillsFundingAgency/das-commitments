using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetOverlapRequestsResponse
    {
        public long? DraftApprenticeshipId { get; set; }
        public long? PreviousApprenticeshipId { get; set; }
        public DateTime? CreatedOn { get; set; }
    }
}
