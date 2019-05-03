using System;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.Extensions;

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
            return GetStatusOn(date) == Apprenticeships.Api.Types.TrainingProgrammeStatus.Active;
        }

        public Apprenticeships.Api.Types.TrainingProgrammeStatus GetStatusOn(DateTime date)
        {
            var dateOnly = date.Date;

            if (EffectiveFrom.HasValue && EffectiveFrom.Value.FirstOfMonth() > dateOnly)
                return Apprenticeships.Api.Types.TrainingProgrammeStatus.Pending;

            if (!EffectiveTo.HasValue || EffectiveTo.Value >= dateOnly)
                return Apprenticeships.Api.Types.TrainingProgrammeStatus.Active;

            return Apprenticeships.Api.Types.TrainingProgrammeStatus.Expired;
        }
    }
}
