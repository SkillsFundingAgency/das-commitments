using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PauseApprenticeship;

public class PauseApprenticeshipCommand : IRequest
{
    public long ApprenticeshipId { get; set; }

    public UserInfo UserInfo { get; set; }
}