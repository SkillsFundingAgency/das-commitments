using System;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class UpdateEndDateOfCompletedRecordRequest : SaveDataRequest
    {
        public long ApprenticeshipId { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
