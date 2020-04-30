using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortWithChangeOfPartyDeletedEvent
    {
        public long CohortId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DateTime DeletedOn { get; }
        public Party DeletedBy { get; }
        public UserInfo UserInfo { get; }

        public CohortWithChangeOfPartyDeletedEvent(long cohortId, long changeOfPartyRequestId, DateTime deletedOn, Party deletedBy, UserInfo userInfo)
        {
            CohortId = cohortId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            DeletedOn = deletedOn;
            DeletedBy = deletedBy;
            UserInfo = userInfo;
        }
    }
}
