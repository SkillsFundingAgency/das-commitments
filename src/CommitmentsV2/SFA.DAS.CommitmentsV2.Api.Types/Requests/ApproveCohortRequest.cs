using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class ApproveCohortRequest : SaveDataRequest
{
    public Party? RequestingParty { get; set; }
    public string Message { get; set; }
}