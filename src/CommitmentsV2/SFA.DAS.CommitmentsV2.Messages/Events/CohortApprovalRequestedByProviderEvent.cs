﻿namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortApprovalRequestedByProviderEvent
    {
        public long AccountId { get; set; }
        public long ProviderId { get; set; }
        public long CommitmentId { get; set; }
    }
}
