using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks
{
    public class RejectDataLocksRequestChangesCommand : IRequest
    {
        public long ApprenticeshipId { get; }
        public UserInfo UserInfo { get; }

        public RejectDataLocksRequestChangesCommand(long apprenticeshipId, UserInfo userInfo)
        {
            ApprenticeshipId = apprenticeshipId;
            UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        }
    }
}