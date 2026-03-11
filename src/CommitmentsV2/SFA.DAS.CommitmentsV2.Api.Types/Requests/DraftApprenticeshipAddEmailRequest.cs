using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class DraftApprenticeshipAddEmailRequest
{
    public string Email { get; set; }

    public Party Party { get; set; }
}
