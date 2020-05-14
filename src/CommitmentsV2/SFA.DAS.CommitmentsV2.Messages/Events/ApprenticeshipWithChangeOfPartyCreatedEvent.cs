using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ApprenticeshipWithChangeOfPartyCreatedEvent
    {
        public long ApprenticeshipId { get; }
        public long ChangeOfPartyRequestId { get; }
        public DateTime CreatedOn { get; }
        public UserInfo UserInfo { get; }
        public Party LastApprovedBy { get; }

        public ApprenticeshipWithChangeOfPartyCreatedEvent(long apprenticeshipId, long changeOfPartyRequestId, DateTime createdOn, UserInfo userInfo, Party lastApprovedBy)
        {
            ApprenticeshipId = apprenticeshipId;
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            CreatedOn = createdOn;
            UserInfo = userInfo;
            LastApprovedBy = lastApprovedBy;
        }
    }
}
