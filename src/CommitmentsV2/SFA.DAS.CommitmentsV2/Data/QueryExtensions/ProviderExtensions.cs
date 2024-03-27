using System.Linq.Expressions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions
{
    public static class ProviderExtensions
    {
        /// <summary>
        ///     Returns an instance of <see cref="TResponse"/> for the specified provider.
        /// </summary>
        /// <typeparam name="TResponse">The type required as a response</typeparam>
        /// <remarks>
        ///     The returned instance will not be tracked (i.e. the object is expected to be read-only).
        /// </remarks>
        public static Task<TResponse> GetById<TResponse>(
            this DbSet<Provider> query,
            long ukPrn,
            Expression<Func<Provider, TResponse>> select,
            CancellationToken cancellationToken)
        {
            return query
                .AsNoTracking()
                .Where(ale => ale.UkPrn == ukPrn)
                .Select(select)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}