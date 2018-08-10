using System;

namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public class FundingPeriod
    {
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public int FundingCap { get; set; }
    }
}
