using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation;

public class ResendInvitationCommand : IRequest
{
    public ResendInvitationCommand(long apprenticeshipId, UserInfo userInfo)
    {
        ApprenticeshipId = apprenticeshipId;
        UserInfo = userInfo;
    }

    public long ApprenticeshipId { get; }
    public UserInfo UserInfo { get; }
}