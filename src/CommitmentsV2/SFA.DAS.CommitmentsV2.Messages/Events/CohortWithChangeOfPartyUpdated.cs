using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class CohortWithChangeOfPartyUpdatedEvent
    {
        public long CohortId { get; set; }
        public UserInfo UserInfo { get; set; }

        public CohortWithChangeOfPartyUpdatedEvent(long cohortId, UserInfo userInfo)
        {
            CohortId = cohortId;
            UserInfo = userInfo;
        }
    }
}
