using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public interface IFundingPeriod
    {
        DateTime? EffectiveFrom { get; set; }
        DateTime? EffectiveTo { get; set; }
        int FundingCap { get; set; }
    }
}