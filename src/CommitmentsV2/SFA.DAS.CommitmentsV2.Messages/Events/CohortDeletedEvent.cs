using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortDeletedEvent
    {
        public long CohortId { get; }
        public long AccountId { get; }
        public long ProviderId { get; }
        public DateTime DeletedOn { get; }
        public Party ApprovedBy { get; }

        public CohortDeletedEvent(long cohortId, long accountId, long providerId, Party approvedBy, DateTime deletedOn)
        {
            CohortId = cohortId;
            AccountId = accountId;
            ProviderId = providerId;
            ApprovedBy = approvedBy;
            DeletedOn = deletedOn;
        }
    }
}