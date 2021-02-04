using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Types.TrainingProgramme
{
    public class TrainingProgramme
    {
        public string CourseCode { get;  set; }
        public string Name { get; set;}
        public ProgrammeType ProgrammeType { get; set;} 
        public DateTime? EffectiveFrom { get; set;}
        public DateTime? EffectiveTo { get; set;}
        public List<TrainingProgrammeFundingPeriod> FundingPeriods { get; set; }
    }

    public class TrainingProgrammeFundingPeriod
    {
        public int FundingCap { get ; set ; }
        public DateTime? EffectiveTo { get ; set ; }
        public DateTime? EffectiveFrom { get ; set ; }
    }
    
    public enum ProgrammeType
    {
        Standard = 0,
        Framework = 1
    }
}