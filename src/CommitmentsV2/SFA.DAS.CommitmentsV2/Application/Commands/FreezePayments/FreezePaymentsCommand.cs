using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.FreezePayments;

public class FreezePaymentsCommand : IRequest
{
    public long ApprenticeshipId { get; set; }
    public UserInfo UserInfo { get; set; }
    public FreezePaymentsReason? FreezePaymentsReason { get; set; }
}
