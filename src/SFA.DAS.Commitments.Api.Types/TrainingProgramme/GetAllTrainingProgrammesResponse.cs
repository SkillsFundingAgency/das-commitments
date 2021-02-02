using System.Collections.Generic;

namespace SFA.DAS.Commitments.Api.Types.TrainingProgramme
{
    public class GetAllTrainingProgrammesResponse
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get; set; }
        
    }
}