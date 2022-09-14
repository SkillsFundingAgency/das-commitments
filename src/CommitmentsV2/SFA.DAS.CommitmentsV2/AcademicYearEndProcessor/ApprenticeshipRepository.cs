using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class ApprenticeshipRepository : IApprenticeshipRepository
    {
        private readonly ILogger<ApprenticeshipRepository> _logger;
        private readonly ProviderCommitmentsDbContext _dbContext;

        public ApprenticeshipRepository(ILogger<ApprenticeshipRepository> logger,
            ProviderCommitmentsDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;

        }

        public async Task<Apprenticeship> GetApprenticeship(long apprenticeshipId)
        {
            var apprenticeships = from a in this._dbContext.Apprenticeships
                                  join c in _dbContext.Cohorts
                              on a.CommitmentId equals c.Id
                                  where a.Id == apprenticeshipId
                                  select new Apprenticeship
                                  {
                                      EmployerAccountId = c.EmployerAccountId,
                                      ProviderId = c.ProviderId,
                                      Id = a.Id,
                                  };

            var apprenticeship = await apprenticeships.FirstOrDefaultAsync();

            if (apprenticeship != null) return apprenticeship;

            var draftsApprenticeships = from a in this._dbContext.DraftApprenticeships
                                        join c in _dbContext.Cohorts
                                            on a.CommitmentId equals c.Id
                                        where a.Id == apprenticeshipId
                                        select new Apprenticeship
                                        {
                                            EmployerAccountId = c.EmployerAccountId,
                                            ProviderId = c.ProviderId,
                                            Id = a.Id,
                                        };

            var draftsApprenticeship = await draftsApprenticeships.FirstOrDefaultAsync();

            return draftsApprenticeship;
        }
    }
}
