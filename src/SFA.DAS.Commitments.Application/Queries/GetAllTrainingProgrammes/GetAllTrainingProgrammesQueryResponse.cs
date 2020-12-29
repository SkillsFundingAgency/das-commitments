using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Application.Queries.GetAllTrainingProgrammes
{
    public class GetAllTrainingProgrammesQueryResponse
    {
        public IEnumerable<ITrainingProgramme> TrainingProgrammes { get ; set ; }
    }
}