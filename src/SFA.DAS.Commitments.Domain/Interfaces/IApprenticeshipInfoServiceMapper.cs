using System.Collections.Generic;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IApprenticeshipInfoServiceMapper
    {
        FrameworksView MapFrom(List<FrameworkSummary> frameworks);
        StandardsView MapFrom(List<StandardSummary> standards);
    }
}