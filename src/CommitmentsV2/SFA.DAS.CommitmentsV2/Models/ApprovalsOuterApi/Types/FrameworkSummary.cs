using Newtonsoft.Json;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types
{
    public class FrameworkSummary
    {
        public string Id { get ; set ; }
        public string FrameworkName { get ; set ; }
        public string PathwayName { get ; set ; }
        public string Title { get ; set ; }
        public int Level { get ; set ; }
        public int FrameworkCode { get ; set ; }
        [JsonProperty("progType")]
        public int ProgrammeType { get ; set ; }
        public int PathwayCode { get ; set ; }
        public int Duration { get ; set ; }
        [JsonProperty("currentFundingCap")]
        public int MaxFunding { get ; set ; }
        public DateTime? EffectiveFrom { get ; set ; }
        public DateTime? EffectiveTo { get ; set ; }
        public IEnumerable<FundingPeriodItem> FundingPeriods { get ; set ; }
    }

    public class FrameworkResponse
    {
        public IEnumerable<FrameworkSummary> Frameworks { get; set; }
    }
}