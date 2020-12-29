using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Queries.GetAllStandardTrainingProgrammes
{
    public class GetAllStandardTrainingProgrammesQueryResponse
    {
        public IEnumerable<ITrainingProgramme> TrainingProgrammes { get ; set ; }
    }
}    