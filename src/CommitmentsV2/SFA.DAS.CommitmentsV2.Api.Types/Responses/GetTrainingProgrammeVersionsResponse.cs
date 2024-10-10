using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetTrainingProgrammeVersionsResponse
{
    public IEnumerable<TrainingProgramme> TrainingProgrammeVersions { get; set; }
}