using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards
{
    public class GetAllTrainingProgrammeStandardsQueryResult
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get ; set ; }
    }
}