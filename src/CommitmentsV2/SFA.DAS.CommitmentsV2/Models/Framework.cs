using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class Framework
    {
        public string Id { get; set; }
        public int FrameworkCode { get; set; }
        public string FrameworkName { get; set; }
        public int Level { get; set; }
        public int PathwayCode { get; set; }
        public string PathwayName { get; set; }
        public int ProgrammeType { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int MaxFunding { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public virtual List<FrameworkFundingPeriod> FundingPeriods { get; set; }
    }
    public class FrameworkFundingPeriod : IFundingPeriod
    {
        public string Id { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int FundingCap { get; set; }
        public virtual Framework Framework { get ; set ; }
    }
}