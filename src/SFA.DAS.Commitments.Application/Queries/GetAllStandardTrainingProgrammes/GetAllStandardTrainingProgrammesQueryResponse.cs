using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Queries.GetAllStandardTrainingProgrammes
{
    public class GetAllStandardTrainingProgrammesQueryResponse
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get ; set ; }
    }
}    