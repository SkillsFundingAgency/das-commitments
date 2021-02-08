using System;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.Api.Types
{
    public class FundingPeriodItem
    {
        [JsonIgnore]
        public int StandardId { get; set; }
        [JsonIgnore]
        public string FrameworkId { get; set; }
        public DateTime? EffectiveFrom { get ; set ; }
        public DateTime? EffectiveTo { get ; set ; }
        [JsonProperty("maxEmployerLevyCap")]
        public int FundingCap { get ; set ; }
    }
}