using System.Linq.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions
{
    public static class CohortExtensions
    {
        /// <summary>
        ///     Returns an instance of <see cref="TResponse"/> for the specified cohort.
        /// </summary>
        /// <typeparam name="TResponse">The type required as a response</typeparam>
        /// <remarks>
        ///     The returned instance will not be tracked (i.e. the object is expected to be read-only).
        /// </remarks>
        public static Task<TResponse> GetById<TResponse>(
            this DbSet<Cohort> query,
            long cohortId,
            Expression<Func<Cohort, TResponse>> select,
            CancellationToken cancellationToken)
        {
            return query.GetById<Cohort, TResponse>(cohort => cohort.Id == cohortId, select, cancellationToken);
        }
    }
}