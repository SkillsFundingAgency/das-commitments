using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.Api.Types
{
    public class ProviderSummary
    {
        public int Ukprn { get; set; }
        [JsonProperty("name")]
        public string ProviderName { get; set; }
    }

    public class ProviderResponse
    {
        public IEnumerable<ProviderSummary> Providers { get; set; }
    }
}