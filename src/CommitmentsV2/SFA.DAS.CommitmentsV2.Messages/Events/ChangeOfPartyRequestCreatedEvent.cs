using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ChangeOfPartyRequestCreatedEvent
    {
        public ChangeOfPartyRequestCreatedEvent(long changeOfPartyRequestId, UserInfo userInfo, bool hasOltd)
        {
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            UserInfo = userInfo;
            HasOltd = hasOltd;
        }

        public long ChangeOfPartyRequestId { get; }
        public bool HasOltd { get; set; }
        public UserInfo UserInfo { get; }
    }
}
