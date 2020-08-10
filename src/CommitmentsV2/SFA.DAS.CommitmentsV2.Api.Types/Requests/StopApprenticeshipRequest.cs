using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class StopApprenticeshipRequest : SaveDataRequest
    {
        public long AccountId { get; set; }
        public DateTime StopDate { get; set; }
    }
}