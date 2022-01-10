using System;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipResendInvitationEvent
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ResendOn { get; set; }
    }
}
