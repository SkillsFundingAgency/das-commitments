using System;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class ApprenticeshipResendInvitationCommand
    {
        public long ApprenticeshipId { get; set; }
        public DateTime ResendOn { get; set; }
    }
}
