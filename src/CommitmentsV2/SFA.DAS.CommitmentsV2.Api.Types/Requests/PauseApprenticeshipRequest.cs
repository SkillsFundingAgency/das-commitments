using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class PauseApprenticeshipRequest : SaveDataRequest
    {
        public long ApprenticeshipId { get; set; }
    }
}
