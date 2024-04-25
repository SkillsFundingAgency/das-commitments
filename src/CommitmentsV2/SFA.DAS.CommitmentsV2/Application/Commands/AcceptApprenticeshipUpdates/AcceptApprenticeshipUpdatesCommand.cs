using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AcceptApprenticeshipUpdates
{
    public class AcceptApprenticeshipUpdatesCommand : IRequest
    {
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public UserInfo UserInfo { get; set; }
        public Party Party { get; }

        public AcceptApprenticeshipUpdatesCommand()
        {
            Party = Party.None;
        }

        public AcceptApprenticeshipUpdatesCommand(Party party, long accountId, long apprenticeshipId, UserInfo userInfo)
        {
            Party = party;
            AccountId = accountId;
            ApprenticeshipId = apprenticeshipId;
            UserInfo = userInfo;
        }
    }
}