using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Data;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Models;
using Microsoft.EntityFrameworkCore;

namespace SFA.DAS.CommitmentsV2.Infrastructure.Data
{
    public class ApprenticeshipUpdateRepository : IApprenticeshipUpdateRepository
    {
        private readonly ILogger<ApprenticeshipUpdateRepository> _logger;
        private readonly ProviderCommitmentsDbContext _dbContext;
        public ApprenticeshipUpdateRepository(
            ILogger<ApprenticeshipUpdateRepository> logger,
            ProviderCommitmentsDbContext dbContext
           ) 
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        public async Task<int> ExpireApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate)
        {
            apprenticeshipUpdate.Status = ApprenticeshipUpdateStatus.Expired;

            var apprenticeship = await _dbContext.Apprenticeships.SingleAsync(a => a.Id == apprenticeshipUpdate.ApprenticeshipId);

            apprenticeship.PendingUpdateOriginator = null;

            return await _dbContext.SaveChangesAsync();
        }
        public async Task<IEnumerable<ApprenticeshipUpdate>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate)
        {

            _logger.LogInformation("Getting all expired apprenticeship update");

            List<ApprenticeshipUpdate> results = await (from au in _dbContext.ApprenticeshipUpdates
                                                        join a in _dbContext.Apprenticeships
                                                               on au.ApprenticeshipId equals a.Id
                                                        where au.Status == ApprenticeshipUpdateStatus.Pending && a.StartDate < currentAcademicYearStartDate
                                                        select au).ToListAsync().ConfigureAwait(false);

            return results;
        }
    }
}
