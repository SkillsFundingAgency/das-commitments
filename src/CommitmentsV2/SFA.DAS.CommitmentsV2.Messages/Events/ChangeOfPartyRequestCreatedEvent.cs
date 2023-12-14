using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class ChangeOfPartyRequestCreatedEvent
    {
        public ChangeOfPartyRequestCreatedEvent(long changeOfPartyRequestId, UserInfo userInfo, bool hasOverlappingTrainingDates)
        {
            ChangeOfPartyRequestId = changeOfPartyRequestId;
            UserInfo = userInfo;
            HasOverlappingTrainingDates = hasOverlappingTrainingDates;
        }

        public long ChangeOfPartyRequestId { get; }
        public bool HasOverlappingTrainingDates { get; set; }
        public UserInfo UserInfo { get; }
    }
}
