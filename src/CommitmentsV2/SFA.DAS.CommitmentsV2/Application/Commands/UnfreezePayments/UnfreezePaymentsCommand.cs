using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UnfreezePayments;

public class UnfreezePaymentsCommand : IRequest
{
    public long ApprenticeshipId { get; set; }
    public UserInfo UserInfo { get; set; }
}
