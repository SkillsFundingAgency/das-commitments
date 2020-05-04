using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortFullyApprovedEvent
    {
        public long CohortId { get; }
        public long AccountId { get; }
        public long ProviderId { get; }
        public DateTime UpdatedOn { get; }
        public Party LastApprovedBy { get; }
        public long? ChangeOfPartyRequestId { get; }
        public UserInfo UserInfo { get; }

        public CohortFullyApprovedEvent(long cohortId, long accountId, long providerId, DateTime updatedOn, Party lastApprovedBy, long? changeOfPartyRequestId, UserInfo userInfo)
        {
            LastApprovedBy = lastApprovedBy;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            UserInfo = userInfo;
            CohortId = cohortId;
            AccountId = accountId;
            ProviderId = providerId;
            UpdatedOn = updatedOn;
        }
    }
}