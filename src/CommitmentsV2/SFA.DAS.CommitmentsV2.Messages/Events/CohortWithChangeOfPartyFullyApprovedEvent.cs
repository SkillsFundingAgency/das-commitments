using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortWithChangeOfPartyFullyApprovedEvent
    {
        public long CohortId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DateTime ApprovedOn { get; }
        public Party ApprovedBy { get; }
        public UserInfo UserInfo { get; }

        public CohortWithChangeOfPartyFullyApprovedEvent(long cohortId, long changeOfPartyRequestId, DateTime approvedOn, Party approvedBy, UserInfo userInfo)
        {
            CohortId = cohortId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            ApprovedOn = approvedOn;
            ApprovedBy = approvedBy;
            UserInfo = userInfo;
        }
    }
}
