using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Api.Types
{
    public class FrameworkSummary
    {
        public string Id { get ; set ; }
        public string FrameworkName { get ; set ; }
        public string PathwayName { get ; set ; }
        public string Title { get ; set ; }
        public int Level { get ; set ; }
        public int FrameworkCode { get ; set ; }
        public int ProgType { get ; set ; }
        public int PathwayCode { get ; set ; }
        public int Duration { get ; set ; }
        public int CurrentFundingCap { get ; set ; }
        public DateTime? EffectiveFrom { get ; set ; }
        public DateTime? EffectiveTo { get ; set ; }
        public IEnumerable<FundingPeriodItem> FundingPeriods { get ; set ; }
    }
}