using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class FreezePaymentsRequest : SaveDataRequest
{
    public FreezePaymentsReason? FreezePaymentsReason { get; set; }
}
