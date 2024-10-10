using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship;

public class CanAccessApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<CanAccessApprenticeshipQuery, bool>
{
    public Task<bool> Handle(CanAccessApprenticeshipQuery request, CancellationToken cancellationToken)
    {
        return dbContext.Value.Apprenticeships.Include(a=>a.Cohort).AnyAsync(
            a => a.Id == request.ApprenticeshipId &&
                 (request.Party == Party.Employer && a.Cohort.EmployerAccountId == request.PartyId ||
                  request.Party == Party.Provider && a.Cohort.ProviderId == request.PartyId),
            cancellationToken: cancellationToken);
    }
}