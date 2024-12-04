using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

public class ProviderSummary
{
    public int Ukprn { get; set; }
    [JsonProperty("name")]
    public string Name { get; set; }
}

public class ProviderResponse
{
    public IEnumerable<ProviderSummary> Providers { get; set; }
}