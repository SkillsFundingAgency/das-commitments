using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetNewerTrainingProgrammeVersions;

public class GetNewerTrainingProgrammeVersionsQueryResult
{
    public IEnumerable<TrainingProgramme> NewerVersions { get; set; }
}