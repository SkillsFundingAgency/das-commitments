using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;

public class ResumeApprenticeshipCommand : IRequest
{
    public long ApprenticeshipId { get; set; }

    public UserInfo UserInfo { get; set; }
}