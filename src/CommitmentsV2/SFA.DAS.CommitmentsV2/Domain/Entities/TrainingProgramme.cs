using System;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using ProgrammeType = SFA.DAS.CommitmentsV2.Types.ProgrammeType;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class TrainingProgramme
    {
        public string CourseCode { get;  }
        public string Name { get; }
        public ProgrammeType ProgrammeType { get; } 
        public DateTime? EffectiveFrom { get; }
        public DateTime? EffectiveTo { get; }

        public TrainingProgramme(string courseCode, string name, ProgrammeType programmeType, DateTime? effectiveFrom, DateTime? effectiveTo)
        {
            CourseCode = courseCode;
            Name = name;
            ProgrammeType = programmeType;
            EffectiveFrom = effectiveFrom;
            EffectiveTo = effectiveTo;
        }

        public bool IsActiveOn(DateTime date)
        {
            return GetStatusOn(date) == TrainingProgrammeStatus.Active;
        }

        public TrainingProgrammeStatus GetStatusOn(DateTime date)
        {
            var dateOnly = date.Date;

            if (EffectiveFrom.HasValue && EffectiveFrom.Value.FirstOfMonth() > dateOnly)
                return TrainingProgrammeStatus.Pending;

            if (!EffectiveTo.HasValue || EffectiveTo.Value >= dateOnly)
                return TrainingProgrammeStatus.Active;

            return TrainingProgrammeStatus.Expired;
        }
        
    }
}
