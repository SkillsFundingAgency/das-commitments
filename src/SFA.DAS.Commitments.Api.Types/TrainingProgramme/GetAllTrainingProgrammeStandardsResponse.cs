using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Types.TrainingProgramme
{
    public class GetAllTrainingProgrammeStandardsResponse
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get; set; }
    }
}