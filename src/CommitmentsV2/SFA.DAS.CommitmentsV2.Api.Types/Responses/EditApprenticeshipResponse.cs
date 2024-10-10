using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class EditApprenticeshipResponse
{
    public long ApprenticeshipId { get; set; }
    public bool NeedReapproval { get; set; }
}