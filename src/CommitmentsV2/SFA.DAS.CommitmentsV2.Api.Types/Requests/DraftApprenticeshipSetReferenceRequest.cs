using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class DraftApprenticeshipSetReferenceRequest
{
    public string Reference { get; set; }
    public Party Party { get; set; }
}
