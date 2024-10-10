using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests;

public class SendCohortRequest : SaveDataRequest
{
    public Party? RequestingParty { get; set; }
    public string Message { get; set; }
}