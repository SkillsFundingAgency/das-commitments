using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public interface ITrainingProgramme
    {
        string Id { get; set; }
        string Title { get; set; }
        int Level { get; set; }
        int MaxFunding { get; set; }
        DateTime? EffectiveFrom { get; set; }
        DateTime? EffectiveTo { get; set; }
        List<FundingPeriod> FundingPeriods { get; set; }
    }
}
