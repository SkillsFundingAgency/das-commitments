using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ChangeOfPartyRequestCreatedEvent
    {
        public ChangeOfPartyRequestCreatedEvent(long changeOfPartyRequestId, UserInfo userInfo)
        {
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            UserInfo = userInfo;
        }

        public long ChangeOfPartyRequestId { get; }
        public UserInfo UserInfo { get; }
    }
}
