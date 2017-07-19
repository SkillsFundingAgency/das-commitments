using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Domain;

using ApprenticeshipUpdate = SFA.DAS.Commitments.Domain.Entities.ApprenticeshipUpdate;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface IApprenticeshipMapper
    {
        Apprenticeship MapFrom(Domain.Entities.Apprenticeship source, CallerType callerType);
        Domain.Entities.Apprenticeship Map(Apprenticeship source, CallerType callerType);
        IEnumerable<Apprenticeship> MapFrom(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType);
        PriceHistory MapPriceHistory(Domain.Entities.PriceHistory domainPrice);
        ApprenticeshipUpdate MapApprenticeshipUpdate(Types.Apprenticeship.ApprenticeshipUpdate apprenticeshipUpdate);
    }
}
