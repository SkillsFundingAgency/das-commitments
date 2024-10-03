using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResolveDataLocks;

public class AcceptDataLocksRequestChangesCommand : IRequest
{
    public long ApprenticeshipId { get; }
    public UserInfo UserInfo { get; }

    public AcceptDataLocksRequestChangesCommand(long apprenticeshipId, UserInfo userInfo)
    {
        ApprenticeshipId = apprenticeshipId;
        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
    }
}