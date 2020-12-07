using System;

namespace SFA.DAS.Commitments.Domain.Api.Types
{
    public class FundingPeriodItem
    {
        public DateTime? EffectiveFrom { get ; set ; }
        public DateTime? EffectiveTo { get ; set ; }
        public int FundingCap { get ; set ; }
    }
}