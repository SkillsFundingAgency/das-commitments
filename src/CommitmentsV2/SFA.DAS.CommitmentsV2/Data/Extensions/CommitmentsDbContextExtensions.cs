using System.Data.SqlClient;
using System.Threading.Tasks;
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
    }
}