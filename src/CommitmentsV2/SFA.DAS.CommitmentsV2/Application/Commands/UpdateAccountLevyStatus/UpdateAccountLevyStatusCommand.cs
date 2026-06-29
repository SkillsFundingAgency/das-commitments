using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateAccountLevyStatus;

public class UpdateAccountLevyStatusCommand : IRequest
{
    public long AccountId { get; set; }

    public ApprenticeshipEmployerType LevyStatus { get; set; }
}
