using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Api.Types
{
    public class StandardSummary
    {
        public int Id { get ; set ; }
        public int Level { get ; set ; }
        public string Title { get ; set ; }
        public int Duration { get ; set ; }
        public int CurrentFundingCap { get ; set ; }
        public DateTime? EffectiveFrom { get ; set ; }
        public DateTime? LastDateForNewStarts { get ; set ; }
        public IEnumerable<FundingPeriodItem> FundingPeriods { get ; set ; }
    }
}