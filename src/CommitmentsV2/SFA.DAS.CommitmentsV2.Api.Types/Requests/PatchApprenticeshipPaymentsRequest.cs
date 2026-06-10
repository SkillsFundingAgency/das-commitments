using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class PatchApprenticeshipPaymentsRequest : SaveDataRequest
{
    public DateTime? PaymentFreezeDate { get; set; }

    public FreezePaymentsReason? FreezePaymentsReason { get; set; }

    public Party Party { get; set; }
}
