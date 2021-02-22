using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.Api.Types
{
    public class StandardSummary
    {
        public int Id { get ; set ; }
        public int Level { get ; set ; }
        public string Title { get ; set ; }
        public int Duration { get ; set ; }
        [JsonProperty("maxFunding")]
        public int CurrentFundingCap { get ; set ; }
        public DateTime? EffectiveFrom { get ; set ; }
        public DateTime? LastDateForNewStarts { get ; set ; }
        [JsonProperty("apprenticeshipFunding")]
        public IEnumerable<FundingPeriodItem> FundingPeriods { get ; set ; }
    }

    public class StandardResponse
    {
        public IEnumerable<StandardSummary> Standards { get; set; }
    }
}