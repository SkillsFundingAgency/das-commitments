using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Api.Types;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IApprenticeshipInfoServiceMapper
    {
        FrameworksView MapFrom(List<Framework> frameworks);
        StandardsView MapFrom(List<Standard> standards);
    }
}