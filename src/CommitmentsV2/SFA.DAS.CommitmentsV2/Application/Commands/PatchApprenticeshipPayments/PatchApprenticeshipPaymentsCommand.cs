using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.PatchApprenticeshipPayments;

public class PatchApprenticeshipPaymentsCommand : IRequest
{
    public long ApprenticeshipId { get; set; }

    public DateTime? PaymentFreezeDate { get; set; }

    public FreezePaymentsReason? FreezePaymentsReason { get; set; }

    public UserInfo UserInfo { get; set; }

    public Party Party { get; set; }
}
