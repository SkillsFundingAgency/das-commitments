using System.Collections.Generic;

using SFA.DAS.Commitments.Domain;

using Apprenticeship = SFA.DAS.Commitments.Api.Types.Apprenticeship.Apprenticeship;
using PriceHistory = SFA.DAS.Commitments.Api.Types.Apprenticeship.PriceHistory;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface IApprenticeshipMapper
    {
        Apprenticeship MapFrom(Domain.Entities.Apprenticeship source, CallerType callerType);
        Domain.Entities.Apprenticeship Map(Apprenticeship source, CallerType callerType);
        IEnumerable<Apprenticeship> MapFrom(IEnumerable<Domain.Entities.Apprenticeship> source, CallerType callerType);
        PriceHistory MapPriceHistory(Domain.Entities.PriceHistory domainPrice);
        Domain.Entities.ApprenticeshipUpdate MapApprenticeshipUpdate(Types.Apprenticeship.ApprenticeshipUpdate apprenticeshipUpdate);
        Types.Apprenticeship.ApprenticeshipUpdate MapApprenticeshipUpdate(Domain.Entities.ApprenticeshipUpdate data);
    }
}
