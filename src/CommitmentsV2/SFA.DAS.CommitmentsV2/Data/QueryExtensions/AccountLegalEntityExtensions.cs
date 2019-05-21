using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions
{
    public static class AccountLegalEntityExtensions
    {
        /// <summary>
        ///     Returns an instance of <see cref="TResponse"/> for the specified account legal entity.
        /// </summary>
        /// <typeparam name="TResponse">The type required as a response</typeparam>
        /// <remarks>
        ///     The returned instance will not be tracked (i.e. the object is expected to be read-only).
        /// </remarks>
        public static Task<TResponse> GetById<TResponse>(
            this DbSet<AccountLegalEntity> query,
            long accountLegalEntityId,
            Expression<Func<AccountLegalEntity, TResponse>> select,
            CancellationToken cancellationToken)
        {
            return query.GetById<AccountLegalEntity, TResponse>(ale => ale.Id == accountLegalEntityId, select, cancellationToken);
        }

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

        public static Task<TResponse> GetById<TEntity, TResponse>(
            this DbSet<TEntity> query,
            Expression<Func<TEntity, bool>> where,
            Expression<Func<TEntity, TResponse>> select,
            CancellationToken cancellationToken) where TEntity : class
        {
            return query
                .AsNoTracking()
                .Where(where)
                .Select(select)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}