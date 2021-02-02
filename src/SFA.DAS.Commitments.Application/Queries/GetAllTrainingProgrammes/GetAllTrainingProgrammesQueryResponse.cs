using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Queries.GetAllTrainingProgrammes
{
    public class GetAllTrainingProgrammesQueryResponse
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get ; set ; }
    }
}