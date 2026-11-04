using System.Collections.Generic;
using Newtonsoft.Json;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class PutApprovalsResponse
{
    public List<ApprovalFieldChange> Changes { get; set; }
    public List<PriceRecord> Prices { get; set; }
}

public class ApprovalFieldChange
{
    public string ChangeType { get; set; }
    public string ApprovalStatus { get; set; }
    public string Reason { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public CocData Data { get; set; }
}
