using SFA.DAS.CommitmentsV2.Models;
using System.Linq.Expressions;

namespace SFA.DAS.CommitmentsV2.Data.QueryExtensions;

public static class ApprenticeshipExtensions
{
    /// <summary>
    ///     Returns an instance of <see cref="TResponse"/> for the specified draft apprenticeship.
    /// </summary>
    /// <typeparam name="TResponse">The type required as a response</typeparam>
    /// <remarks>
    ///     The returned instance will not be tracked (i.e. the object is expected to be read-only).
    /// </remarks>
    public static Task<TResponse> GetById<TResponse>(
        this IQueryable<Apprenticeship> query,
        long apprenticeshipId,
        Expression<Func<Apprenticeship, TResponse>> select,
        CancellationToken cancellationToken)
    {
        return query.GetById<Apprenticeship, TResponse>(a => a.Id == apprenticeshipId, select, cancellationToken);
    }
}