using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.CanAccessCohort
{
    public class CanAccessCohortQueryHandler : IRequestHandler<CanAccessCohortQuery, bool>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public CanAccessCohortQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> Handle(CanAccessCohortQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Value.Cohorts.AnyAsync(
                c => c.Id == request.CohortId &&
                     (request.Party == Party.Employer && c.EmployerAccountId == request.PartyId ||
                      request.Party == Party.Provider && c.ProviderId == request.PartyId),
                cancellationToken: cancellationToken);
        }
    }
}