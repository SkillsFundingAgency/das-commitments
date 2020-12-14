using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Events
{
    public class UpdateChangeOfPartyRequestEvent
    {
        public long CohortId { get; set; }
        public UserInfo UserInfo { get; set; }

        public UpdateChangeOfPartyRequestEvent(long cohortId, UserInfo userInfo)
        {
            CohortId = cohortId;
            UserInfo = userInfo;
        }
    }
}
