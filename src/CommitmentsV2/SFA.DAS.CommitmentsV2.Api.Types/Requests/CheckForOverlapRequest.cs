using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class ValidateUlnOverlapRequest : SaveDataRequest
    {
        public long? ApprenticeshipId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string ULN { get; set; }
    }
}
