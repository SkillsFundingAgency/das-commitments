using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class PatchApprenticeshipPaymentsRequest : SaveDataRequest
{
    public bool FreezePayments { get; set; }

    public FreezePaymentsReason? FreezePaymentsReason { get; set; }
}
