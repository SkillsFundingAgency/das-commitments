using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipUpdatedEmailAddressEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ApprovedOn { get; set; }
    }
}
