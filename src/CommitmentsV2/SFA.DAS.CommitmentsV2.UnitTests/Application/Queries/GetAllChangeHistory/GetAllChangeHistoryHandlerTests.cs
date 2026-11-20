using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllChangeHistory;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAllChangeHistory;

public class GetAllChangeHistoryHandlerTests
{
    [Test]
    public async Task Handle_WithProviderId_ShouldReturnAllChangeHistory()
    {
        var fixtures = new GetAllChangeHistoryForProviderQueryHandlerTestFixture();

        var response = await fixtures.GetResponse(new GetAllChangeHistoryForProviderQuery() { ProviderId = fixtures.ProviderId });

        response.ChangeHistory.Should().NotBeNull();
        response.ChangeHistory.Count.Should().Be(fixtures.SeedChangeHistory.Count);
        response.ChangeHistory.Should().BeEquivalentTo(fixtures.SeedChangeHistory.Select(x => new ChangeHistory
        {
            Id = x.Id,
            ApprenticeshipId = x.ApprenticeshipId,
            ChangeType = x.ChangeType,
            AppliedDate = x.AppliedDate,
            LearnerName = x.LearnerName,
            Description = x.Description,
            Created = x.Created,
            EmployerName = x.EmployerName,
        }));
    }

    [Test]
    public async Task Handle_WithNoMatchingProviderId_ShouldReturnEmptyChangeHistory()
    {
        var fixtures = new GetAllChangeHistoryForProviderQueryHandlerTestFixture().GenerateChangeHistoryWithProviderId(0);

        var response = await fixtures.GetResponse(new GetAllChangeHistoryForProviderQuery() { ProviderId = fixtures.ProviderId });
        response.ChangeHistory.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_WithPaging_ShouldReturnRequestedPageNewestFirst()
    {
        var fixtures = new GetAllChangeHistoryForProviderQueryHandlerTestFixture().SetSequentialCreatedDates();

        var response = await fixtures.GetResponse(new GetAllChangeHistoryForProviderQuery { ProviderId = fixtures.ProviderId, PageNumber = 2, PageSize = 2 });

        response.ChangeHistory.Should().HaveCount(1);
        response.ChangeHistory.Single().Id.Should().Be(fixtures.SeedChangeHistory[0].Id);
        response.TotalCount.Should().Be(3);
    }

    [Test]
    public async Task Handle_WithPageBeyondLastPage_ShouldReturnEmptyPageWithTotalCount()
    {
        var fixtures = new GetAllChangeHistoryForProviderQueryHandlerTestFixture().SetSequentialCreatedDates();

        var response = await fixtures.GetResponse(new GetAllChangeHistoryForProviderQuery { ProviderId = fixtures.ProviderId, PageNumber = 5, PageSize = 2 });

        response.ChangeHistory.Should().BeEmpty();
        response.TotalCount.Should().Be(3);
    }

    [Test]
    public async Task Handle_WithPaging_ShouldReturnAvailableFromAsEarliestChangeAcrossAllPages()
    {
        var fixtures = new GetAllChangeHistoryForProviderQueryHandlerTestFixture().SetSequentialCreatedDates();

        var response = await fixtures.GetResponse(new GetAllChangeHistoryForProviderQuery { ProviderId = fixtures.ProviderId, PageNumber = 1, PageSize = 1 });

        response.ChangeHistory.Should().HaveCount(1);
        response.AvailableFrom.Should().Be(fixtures.SeedChangeHistory.Min(x => x.Created));
    }

    [Test]
    public async Task Handle_WithNoMatchingProviderId_ShouldReturnZeroTotalCountAndNullAvailableFrom()
    {
        var fixtures = new GetAllChangeHistoryForProviderQueryHandlerTestFixture().GenerateChangeHistoryWithProviderId(0);

        var response = await fixtures.GetResponse(new GetAllChangeHistoryForProviderQuery { ProviderId = fixtures.ProviderId });

        response.TotalCount.Should().Be(0);
        response.AvailableFrom.Should().BeNull();
    }

    public class GetAllChangeHistoryForProviderQueryHandlerTestFixture
    {
        private readonly Fixture _autoFixture;
        public long ProviderId { get; set; }

        public GetAllChangeHistoryForProviderQueryHandlerTestFixture()
        {
            _autoFixture = new Fixture();
            SeedChangeHistory = _autoFixture.Create<List<LearningChangeHistory>>();
            ProviderId = _autoFixture.Create<long>();

            SeedChangeHistory.ForEach(x => x.UKPRN = ProviderId);
        }

        public List<LearningChangeHistory> SeedChangeHistory { get; }

        public Task<GetAllChangeHistoryForProviderQueryResult> GetResponse(GetAllChangeHistoryForProviderQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetAllChangeHistoryForProviderQueryHandler(lazy);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options;

            using var dbContext = new ProviderCommitmentsDbContext(options);
            dbContext.Database.EnsureCreated();
            SeedData(dbContext);
            return action(dbContext);
        }

        public GetAllChangeHistoryForProviderQueryHandlerTestFixture GenerateChangeHistoryWithProviderId(long providerId)
        {
            SeedChangeHistory.ForEach(x => x.UKPRN = providerId);
            return this;
        }

        public GetAllChangeHistoryForProviderQueryHandlerTestFixture SetSequentialCreatedDates()
        {
            var startDate = new DateTime(2026, 1, 1);
            for (var i = 0; i < SeedChangeHistory.Count; i++)
            {
                SeedChangeHistory[i].Created = startDate.AddDays(i);
            }

            return this;
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.LearningChangeHistory.AddRange(SeedChangeHistory);

            dbContext.SaveChanges(true);
        }
    }
}