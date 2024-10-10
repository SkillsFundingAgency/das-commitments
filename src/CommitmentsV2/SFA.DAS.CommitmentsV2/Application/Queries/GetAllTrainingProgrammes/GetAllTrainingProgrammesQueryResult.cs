using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammes;

public class GetAllTrainingProgrammesQueryResult
{
    public IEnumerable<TrainingProgramme> TrainingProgrammes { get ; set ; }
}