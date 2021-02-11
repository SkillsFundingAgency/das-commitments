using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipUlnUpdatedEvent
    {
        public long ApprenticeshipId { get; set; }
        public string Uln { get; set; }
        public DateTime UpdatedOn { get; set; }

        public ApprenticeshipUlnUpdatedEvent(long apprenticeshipId, string uln, DateTime updatedOn )
        {
            ApprenticeshipId = apprenticeshipId;
            Uln = uln;
            UpdatedOn = updatedOn;
        }
    }
}
