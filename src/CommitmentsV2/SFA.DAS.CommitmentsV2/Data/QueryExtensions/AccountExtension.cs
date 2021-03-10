using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions
{
    public static class AccountExtension
    {
        /// <summary>
        ///     Returns an instance of <see cref="TResponse"/> for the specified account legal entity.
        /// </summary>
        /// <typeparam name="TResponse">The type required as a response</typeparam>
        /// <remarks>
        ///     The returned instance will not be tracked (i.e. the object is expected to be read-only).
        /// </remarks>
        public static Task<TResponse> GetById<TResponse>(
            this DbSet<Account> query,
            long accountId,
            Expression<Func<Account, TResponse>> select,
            CancellationToken cancellationToken)
        {
            return query.GetById<Account, TResponse>(a => a.Id == accountId, select,
                cancellationToken);
        }
    }
}