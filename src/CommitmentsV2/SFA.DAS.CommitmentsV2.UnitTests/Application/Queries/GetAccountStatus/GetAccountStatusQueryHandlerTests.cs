using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountStatus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using Microsoft.Extensions.Logging;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountStatus;

[TestFixture]
public class GetAccountStatusQueryHandlerTests
{
    [Test]
    public async Task Handle_Should_Return_Active_For_NonNewStart_Active_Apprenticeship()
    {
        var utcNow = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

        var fixture = new GetAccountStatusQueryHandlerTestsFixture()
            .WithDefaultLags(completionLag: 3, startLag: 60, newStartWindow: 3)
            .WithUtcNow(utcNow)
            .AddCohort(out var cohortId, providerId: 10020001)
            .AddApprenticeship(cohortId, courseCode: "430",
                status: PaymentStatus.Active,
                startDate: utcNow.AddMonths(-4)) // non-new-start active
            .Build();

        var response = await fixture.GetResponse();

        Assert.Multiple(() =>
        {
            Assert.That(response.Active, Has.Count.EqualTo(1));
            Assert.That(response.Active,
                Has.Exactly(1).Matches<AccountStatusProviderCourse>(x =>
                    x.Ukprn == 10020001L && x.CourseCode == "430"));

            Assert.That(response.NewStart, Is.Empty);
            Assert.That(response.Completed, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_Should_Return_NewStart_For_Recent_Active_Apprenticeship()
    {
        var utcNow = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

        var fixture = new GetAccountStatusQueryHandlerTestsFixture()
            .WithDefaultLags(3, 60, 3)
            .WithUtcNow(utcNow)
            .AddCohort(out var cohortId, providerId: 10030002)
            .AddApprenticeship(cohortId, "532",
                PaymentStatus.Active,
                startDate: utcNow.AddMonths(-1)) // within newStart window
            .Build();

        var response = await fixture.GetResponse();

        Assert.Multiple(() =>
        {
            Assert.That(response.NewStart, Has.Count.EqualTo(1));
            Assert.That(response.NewStart,
                Has.Exactly(1).Matches<AccountStatusProviderCourse>(x =>
                    x.Ukprn == 10030002L && x.CourseCode == "532"));

            Assert.That(response.Active, Is.Empty);
            Assert.That(response.Completed, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_Should_Return_Completed_For_Recent_Completed_Apprenticeship_And_Also_Appear_In_Active()
    {
        // Note: with current handler logic, completed rows can also set IsActive=1 (non-new-start branch).
        var utcNow = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

        var fixture = new GetAccountStatusQueryHandlerTestsFixture()
            .WithDefaultLags(3, 60, 3)
            .WithUtcNow(utcNow)
            .AddCohort(out var cohortId, providerId: 10040003)
            .AddApprenticeship(cohortId, "167",
                PaymentStatus.Completed,
                startDate: utcNow.AddMonths(-10),
                completionDate: utcNow.AddMonths(-1)) // within completion lag
            .Build();

        var response = await fixture.GetResponse();

        Assert.Multiple(() =>
        {
            Assert.That(response.Completed, Has.Count.EqualTo(1));
            Assert.That(response.Completed,
                Has.Exactly(1).Matches<AccountStatusProviderCourse>(x =>
                    x.Ukprn == 10040003L && x.CourseCode == "167"));

            Assert.That(response.Active, Has.Count.EqualTo(1));
            Assert.That(response.Active,
                Has.Exactly(1).Matches<AccountStatusProviderCourse>(x =>
                    x.Ukprn == 10040003L && x.CourseCode == "167"));

            Assert.That(response.NewStart, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_Should_Exclude_Completed_Outside_CompletionLag()
    {
        var utcNow = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

        var fixture = new GetAccountStatusQueryHandlerTestsFixture()
            .WithDefaultLags(3, 60, 3)
            .WithUtcNow(utcNow)
            .AddCohort(out var cohortId, providerId: 10050004)
            .AddApprenticeship(cohortId, "222",
                PaymentStatus.Completed,
                startDate: utcNow.AddMonths(-10),
                completionDate: utcNow.AddMonths(-6)) // outside lag
            .Build();

        var response = await fixture.GetResponse();

        Assert.Multiple(() =>
        {
            Assert.That(response.Completed, Is.Empty);
            Assert.That(response.Active, Is.Empty);
            Assert.That(response.NewStart, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_Should_Exclude_Starts_Before_StartLag()
    {
        var utcNow = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

        var fixture = new GetAccountStatusQueryHandlerTestsFixture()
            .WithDefaultLags(3, 60, 3)
            .WithUtcNow(utcNow)
            .AddCohort(out var cohortId, providerId: 10060005)
            .AddApprenticeship(cohortId, "325",
                PaymentStatus.Active,
                startDate: utcNow.AddMonths(-61)) // before start-lag
            .Build();

        var response = await fixture.GetResponse();

        Assert.Multiple(() =>
        {
            Assert.That(response.Active, Is.Empty);
            Assert.That(response.NewStart, Is.Empty);
            Assert.That(response.Completed, Is.Empty);
        });
    }

    [Test]
    public async Task Handle_Should_DeDuplicate_By_Ukprn_And_CourseCode_Per_Bucket()
    {
        var utcNow = new DateTime(2025, 08, 01, 0, 0, 0, DateTimeKind.Utc);

        var fixture = new GetAccountStatusQueryHandlerTestsFixture()
            .WithDefaultLags(3, 60, 3)
            .WithUtcNow(utcNow)
            .AddCohort(out var cohortId, providerId: 10070006)
            // Two actives same (ukprn, course)
            .AddApprenticeship(cohortId, "191", PaymentStatus.Active, startDate: utcNow.AddMonths(-5))
            .AddApprenticeship(cohortId, "191", PaymentStatus.Active, startDate: utcNow.AddMonths(-6))
            // One completed same (ukprn, course) within lag
            .AddApprenticeship(cohortId, "191", PaymentStatus.Completed, startDate: utcNow.AddMonths(-10), completionDate: utcNow.AddMonths(-1))
            .Build();

        var response = await fixture.GetResponse();

        Assert.Multiple(() =>
        {
            Assert.That(response.Active,
                Has.Exactly(1).Matches<AccountStatusProviderCourse>(x =>
                    x.Ukprn == 10070006L && x.CourseCode == "191"));

            Assert.That(response.Completed,
                Has.Exactly(1).Matches<AccountStatusProviderCourse>(x =>
                    x.Ukprn == 10070006L && x.CourseCode == "191"));
        });
    }
}

internal sealed class GetAccountStatusQueryHandlerTestsFixture
{
    private readonly Fixture _auto = new();
    private readonly List<Cohort> _cohorts = new();
    private readonly List<Apprenticeship> _apprenticeships = new();

    public long EmployerAccountId { get; }
    public DateTime UtcNow { get; private set; }
    public int CompletionLag { get; private set; }
    public int StartLag { get; private set; }
    public int NewStartWindow { get; private set; }

    public GetAccountStatusQueryHandlerTestsFixture()
    {
        EmployerAccountId = _auto.Create<long>();
        UtcNow = DateTime.UtcNow;
    }

    public GetAccountStatusQueryHandlerTestsFixture WithUtcNow(DateTime utcNow) { UtcNow = utcNow; return this; }
    public GetAccountStatusQueryHandlerTestsFixture WithDefaultLags(int completionLag, int startLag, int newStartWindow)
    {
        CompletionLag = completionLag;
        StartLag = startLag;
        NewStartWindow = newStartWindow;
        return this;
    }

    public GetAccountStatusQueryHandlerTestsFixture AddCohort(out long cohortId, long providerId)
    {
        var c = new Cohort
        {
            Id = _auto.Create<long>(),
            EmployerAccountId = EmployerAccountId,
            ProviderId = providerId,
            IsDeleted = false
        };
        cohortId = c.Id;
        _cohorts.Add(c);
        return this;
    }

    public GetAccountStatusQueryHandlerTestsFixture AddApprenticeship(long cohortId, string courseCode, PaymentStatus status, DateTime startDate, DateTime? completionDate = null)
    {
        var a = new Apprenticeship
        {
            Id = _auto.Create<long>(),
            CommitmentId = cohortId,
            CourseCode = courseCode,
            StartDate = startDate,
            CompletionDate = completionDate,
            PaymentStatus = status
        };
        _apprenticeships.Add(a);
        return this;
    }

    public TestContext Build() => new TestContext(
        EmployerAccountId, UtcNow, CompletionLag, StartLag, NewStartWindow, _cohorts, _apprenticeships);
}

internal sealed class TestContext
{
    public long EmployerAccountId { get; }
    public DateTime UtcNow { get; }
    public int CompletionLag { get; }
    public int StartLag { get; }
    public int NewStartWindow { get; }

    private readonly List<Cohort> _cohorts;
    private readonly List<Apprenticeship> _apprenticeships;

    public TestContext(long employerAccountId, DateTime utcNow,
        int completionLag, int startLag, int newStartWindow,
        List<Cohort> cohorts, List<Apprenticeship> apprenticeships)
    {
        EmployerAccountId = employerAccountId;
        UtcNow = utcNow;
        CompletionLag = completionLag;
        StartLag = startLag;
        NewStartWindow = newStartWindow;
        _cohorts = cohorts;
        _apprenticeships = apprenticeships;
    }

    public async Task<GetAccountStatusQueryResult> GetResponse()
    {
        var request = new GetAccountStatusQuery
        {
            AccountId = EmployerAccountId,
            CompletionLag = CompletionLag,
            StartLag = StartLag,
            NewStartWindow = NewStartWindow
        };

        return await RunWithDbContext(async dbContext =>
        {
            var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
            var handler = new GetAccountStatusQueryHandler(lazy, new StubCurrentDateTime(UtcNow));
            return await handler.Handle(request, CancellationToken.None);
        });
    }

    private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
    {
        var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
            .UseLoggerFactory(LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Warning)))
            .Options;

        using var db = new ProviderCommitmentsDbContext(options);
        db.Database.EnsureCreated();
        Seed(db);
        return action(db);
    }

    private void Seed(ProviderCommitmentsDbContext db)
    {
        db.Cohorts.AddRange(_cohorts);
        db.Apprenticeships.AddRange(_apprenticeships);
        db.SaveChanges(true);
    }
}

internal sealed class StubCurrentDateTime : ICurrentDateTime
{
    public StubCurrentDateTime(DateTime utcNow) => UtcNow = utcNow;
    public DateTime UtcNow { get; }
}
