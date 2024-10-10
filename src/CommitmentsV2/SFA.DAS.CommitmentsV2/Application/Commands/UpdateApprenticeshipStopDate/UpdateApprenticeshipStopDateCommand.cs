using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;

public class UpdateApprenticeshipStopDateCommand : IRequest
{
    public long AccountId { get; }
    public long ApprenticeshipId { get; }        
    public DateTime StopDate { get; }
    public UserInfo UserInfo { get; }

    public UpdateApprenticeshipStopDateCommand(long accountId, long apprenticeshipId, DateTime stopDate,  UserInfo userInfo)
    {
        AccountId = accountId;
        ApprenticeshipId = apprenticeshipId;
        StopDate = stopDate;
        UserInfo = userInfo;
    }
}