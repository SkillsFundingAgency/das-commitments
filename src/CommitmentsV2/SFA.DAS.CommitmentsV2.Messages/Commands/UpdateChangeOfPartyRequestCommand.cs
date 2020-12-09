using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands
{
    public class UpdateChangeOfPartyRequestCommand
    {
        public long CohortId { get; set; }
        public UserInfo UserInfo { get; set; }

        public UpdateChangeOfPartyRequestCommand(long cohortId, UserInfo userInfo)
        {
            CohortId = cohortId;
            UserInfo = userInfo;
        }
    }
}
