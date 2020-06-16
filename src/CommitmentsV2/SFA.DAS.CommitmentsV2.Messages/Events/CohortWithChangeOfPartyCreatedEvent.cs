using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortWithChangeOfPartyCreatedEvent
    {
        public long CohortId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DateTime CreatedOn { get; }
        public UserInfo UserInfo { get;  }
        public Party OriginatingParty { get; set; }

        public CohortWithChangeOfPartyCreatedEvent(long cohortId, long changeOfPartyRequestId, Party originatingParty, DateTime createdOn, UserInfo userInfo)
        {
            CohortId = cohortId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            OriginatingParty = originatingParty;
            CreatedOn = createdOn;
            UserInfo = userInfo;
        }
    }
}
