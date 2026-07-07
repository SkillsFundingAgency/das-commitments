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

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.LearningChangeHistory.AddRange(SeedChangeHistory);

            dbContext.SaveChanges(true);
        }
    }
}