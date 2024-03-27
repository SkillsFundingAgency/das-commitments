using System.Linq.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions
{
    public static class DraftApprenticeshipExtensions
    {
        /// <summary>
        ///     Returns an instance of <see cref="TResponse"/> for the specified draft apprenticeship.
        /// </summary>
        /// <typeparam name="TResponse">The type required as a response</typeparam>
        /// <remarks>
        ///     The returned instance will not be tracked (i.e. the object is expected to be read-only).
        /// </remarks>
        public static Task<TResponse> GetById<TResponse>(
            this DbSet<DraftApprenticeship> query,
            long cohortId,
            long draftApprenticeshipId,
            Expression<Func<DraftApprenticeship, TResponse>> select,
            CancellationToken cancellationToken)
        {
            return query.GetById<DraftApprenticeship, TResponse>(draft => draft.CommitmentId == cohortId && draft.Id == draftApprenticeshipId, select, cancellationToken);
        }
    }
}