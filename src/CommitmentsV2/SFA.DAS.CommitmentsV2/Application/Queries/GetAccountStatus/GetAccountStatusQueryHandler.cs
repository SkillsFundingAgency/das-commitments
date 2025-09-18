using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;

public class GetAccountStatusQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ICurrentDateTime currentDateTime) : IRequestHandler<GetAccountStatusQuery, GetAccountStatusQueryResult>
{
    public async Task<GetAccountStatusQueryResult> Handle(GetAccountStatusQuery query,
        CancellationToken cancellationToken)
    {
        var utcNow = currentDateTime.UtcNow;
        var completionCutoff = utcNow.AddMonths(-query.CompletionLag);
        var startLagCutoff = utcNow.AddMonths(-query.StartLag);
        var newStartCutoff = utcNow.AddMonths(-query.NewStartWindow);

        var results = await dbContext.Value.Apprenticeships
            .Include(a => a.Cohort)
            .Where(a =>
                a.Cohort.EmployerAccountId == query.AccountId &&
                (a.PaymentStatus == PaymentStatus.Active || a.PaymentStatus == PaymentStatus.Completed) &&
                a.StartDate > startLagCutoff &&
                !(a.PaymentStatus == PaymentStatus.Completed && a.CompletionDate < completionCutoff)
            )
            .Select(a => new
            {
                a.Cohort.EmployerAccountId,
                a.Cohort.ProviderId,
                a.CourseCode,
                IsCompleted = (a.PaymentStatus == PaymentStatus.Completed && a.CompletionDate >= completionCutoff) ? 1 : 0,
                IsNewStart = (a.PaymentStatus == PaymentStatus.Active && a.StartDate >= newStartCutoff) ? 1 : 0,
                IsActive = (a.PaymentStatus == PaymentStatus.Active && a.StartDate >= newStartCutoff) ? 0 : 1
            })
            .GroupBy(x => new { x.EmployerAccountId, x.ProviderId, x.CourseCode })
            .Select(g => new
            {
                g.Key.EmployerAccountId,
                ukprn = g.Key.ProviderId,
                courseCode = g.Key.CourseCode,
                IsCompleted = g.Max(x => x.IsCompleted),
                IsNewStart = g.Max(x => x.IsNewStart),
                IsActive = g.Max(x => x.IsActive)
            })
            .ToListAsync();

            results = results
                .OrderBy(x => x.EmployerAccountId)
                .ThenBy(x => x.ukprn)
                .ThenBy(x => x.courseCode)
                .ToList();

        return new GetAccountStatusQueryResult
        {
            Active = results
                .Where(r => r.IsActive == 1)
                .Select(r => new AccountStatusProviderCourse { Ukprn = r.ukprn, CourseCode = r.courseCode })
                .DistinctBy(x => (x.Ukprn, x.CourseCode))
                .ToList(),

            Completed = results
                .Where(r => r.IsCompleted == 1)
                .Select(r => new AccountStatusProviderCourse { Ukprn = r.ukprn, CourseCode = r.courseCode })
                .DistinctBy(x => (x.Ukprn, x.CourseCode))
                .ToList(),

            NewStart = results
                .Where(r => r.IsNewStart == 1)
                .Select(r => new AccountStatusProviderCourse { Ukprn = r.ukprn, CourseCode = r.courseCode })
                .DistinctBy(x => (x.Ukprn, x.CourseCode))
                .ToList()
        };
    }
}