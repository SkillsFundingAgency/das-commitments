using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface IApprenticeshipMapper
    {
        Apprenticeship MapFrom(Domain.Entities.Apprenticeship source, CallerType callerType);
        IEnumerable<Apprenticeship> MapFrom(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType);
    }
}
