using System;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.Extensions;

namespace SFA.DAS.CommitmentsV2.Domain.ValueObjects
{
    public class TrainingProgramme
    {
        public string CourseCode { get; private set; }
        public string Name { get; private set; }
        public ProgrammeType ProgrammeType { get; private set; } 
        public DateTime? EffectiveFrom { get; private set; }
        public DateTime? EffectiveTo { get; private set; }

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
