using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.CanAccessApprenticeship
{
    public class CanAccessApprenticeshipQueryHandler : IRequestHandler<CanAccessApprenticeshipQuery, bool>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public CanAccessApprenticeshipQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> Handle(CanAccessApprenticeshipQuery request, CancellationToken cancellationToken)
        {
            return _dbContext.Value.Apprenticeships.Include(a=>a.Cohort).AnyAsync(
                a => a.Id == request.ApprenticeshipId &&
                     (request.Party == Party.Employer && a.Cohort.EmployerAccountId == request.PartyId ||
                      request.Party == Party.Provider && a.Cohort.ProviderId == request.PartyId),
                cancellationToken: cancellationToken);
        }
    }
}