using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortWithChangeOfPartyCreatedEvent
    {
        public long CohortId { get; }
        public long ChangeOfPartyRequestId { get; }
        public Party OriginatingParty { get; set; }
        public ChangeOfPartyRequestType ChangeOfPartyType { get; set; }
        public long ApprenticeshipId { get; set; }
        public DateTime CreatedOn { get; }
        public UserInfo UserInfo { get;  }
        
        public CohortWithChangeOfPartyCreatedEvent(long cohortId, long changeOfPartyRequestId, Party originatingParty, ChangeOfPartyRequestType changeOfPartyRequestType, long apprenticeshipId, DateTime createdOn, UserInfo userInfo)
        {
            CohortId = cohortId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            OriginatingParty = originatingParty;
            ChangeOfPartyType = changeOfPartyRequestType;
            ApprenticeshipId = apprenticeshipId;
            CreatedOn = createdOn;
            UserInfo = userInfo;
        }
    }
}
