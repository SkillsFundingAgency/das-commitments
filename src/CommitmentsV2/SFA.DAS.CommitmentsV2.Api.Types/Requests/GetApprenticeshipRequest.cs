using Microsoft.AspNetCore.Mvc;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class GetApprenticeshipRequest
    {
        [FromQuery]
        public long? AccountId { get; set; }
        [FromQuery]
        public long? ProviderId { get; set; }
        [FromQuery]
        public int PageNumber { get; set; }
        [FromQuery]
        public int PageItemCount { get; set; }
        [FromQuery]
        public string SortField { get; set; }
        [FromQuery]
        public bool ReverseSort { get; set; }
        
    }
}