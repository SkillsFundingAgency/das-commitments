using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetAllTrainingProgrammesResponse
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get; set; }
        
    }
}