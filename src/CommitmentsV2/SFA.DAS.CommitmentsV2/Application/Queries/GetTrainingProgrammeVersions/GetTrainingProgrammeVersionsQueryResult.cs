﻿using SFA.DAS.CommitmentsV2.Types;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetTrainingProgrammeVersions
{
    public class GetTrainingProgrammeVersionsQueryResult
    {
        public IEnumerable<TrainingProgramme> TrainingProgrammes { get; set; }
    }
}