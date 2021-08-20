using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public class Framework : ITrainingProgramme
    {
        public Framework ()
        {
            FundingPeriods = new List<FundingPeriod>();
        }
        public int FrameworkCode { get; set; }
        public string FrameworkName { get; set; }
        public string LarsCode { get; set; }
        public int Level { get; set; }
        public int PathwayCode { get; set; }
        public string PathwayName { get; set; }
        public int ProgrammeType { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        public int MaxFunding { get; set; }
        public DateTime? EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public List<FundingPeriod> FundingPeriods { get; set; }
    }
}