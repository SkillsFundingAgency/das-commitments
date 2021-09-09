using System;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetCalculatedTrainingProgrammeVersion
{
    public class GetTrainingProgrammeOverallStartAndEndDatesQueryResult
    {
        public DateTime? TrainingProgrammeEffectiveFrom { get; set; }
        public DateTime? TrainingProgrammeEffectiveTo { get; set; }
    }
}
