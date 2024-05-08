using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllTrainingProgrammeStandards
{
    public class GetAllTrainingProgrammeStandardsQueryResult
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get ; set ; }
    }
}