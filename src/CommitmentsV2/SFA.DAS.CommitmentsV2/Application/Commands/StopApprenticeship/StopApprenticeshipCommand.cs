using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship;

public class StopApprenticeshipCommand : IRequest
{
    public UserInfo UserInfo { get; }
    public long AccountId{ get; }
    public long ApprenticeshipId { get; }
    public DateTime StopDate { get; }

    public bool MadeRedundant { get; }
    public Party Party { get; set; }

    public StopApprenticeshipCommand(long accountId, long apprenticeshipId, DateTime stopDate, bool madeRedundant, UserInfo userInfo, Party party)
    {
        AccountId = accountId;
        ApprenticeshipId = apprenticeshipId;
        StopDate = stopDate;
        MadeRedundant = madeRedundant;
        UserInfo = userInfo ?? throw new ArgumentNullException(nameof(userInfo));
        Party = party;
    }
}