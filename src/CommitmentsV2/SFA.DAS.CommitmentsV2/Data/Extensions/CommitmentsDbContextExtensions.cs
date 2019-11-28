using System;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Data.Extensions
{
    public static class CommitmentsDbContextExtensions
    {
        public static Task ProcessFullyApprovedCohort(this ProviderCommitmentsDbContext db, long cohortId, long accountId, ApprenticeshipEmployerType apprenticeshipEmployerType)
        {
            return db.ExecuteSqlCommandAsync(
                "EXEC ProcessFullyApprovedCohort @cohortId, @accountId, @apprenticeshipEmployerType",
                new SqlParameter("cohortId", cohortId),
                new SqlParameter("accountId", accountId),
                new SqlParameter("apprenticeshipEmployerType", apprenticeshipEmployerType));
        }

        public static async Task<Cohort> GetCohortWithDraftApprenticeships(this ProviderCommitmentsDbContext db, long cohortId, CancellationToken cancellationToken)
        {
            var cohort = await db.Cohorts.Include(c => c.Apprenticeships).SingleOrDefaultAsync(c => c.Id == cohortId, cancellationToken);
            if (cohort == null) throw new BadRequestException($"Cohort {cohortId} was not found");
            if (cohort.IsApprovedByAllParties) throw new InvalidOperationException($"Cohort {cohortId} is approved by all parties and can't be modified");
            return cohort;
        }
    }
}