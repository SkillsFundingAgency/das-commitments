using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistory;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeHistory;

public class GetChangeHistoryHandlerTests
{
    [Test]
    public async Task Handle_WithApprenticeshipId_ShouldReturnChangeHistory()
    {
        var fixtures = new GetChangeHistoryHandlerTestFixture();

        var response = await fixtures.GetResponse(new GetChangeHistoryQuery() { ApprenticeshipId = fixtures.ApprenticeshipId });

        response.ChangeHistory.Should().NotBeNull();
        response.ChangeHistory.Should().BeEquivalentTo(fixtures.SeedChangeHistory.Select(x => new ChangeHistory
        {
            Id = x.Id,
            ApprenticeshipId = x.ApprenticeshipId,
            ChangeType = x.ChangeType,
            AppliedDate = x.AppliedDate,
            LearnerName = x.LearnerName,
            Description = x.Description,
            Created = x.Created
        }));
    }

    [Test]
    public async Task Handle_WithNoMatchingApprenticeshipId_ShouldReturnEmptyChangeHistory()
    {
        var fixtures = new GetChangeHistoryHandlerTestFixture().GenerateChangeHistoryWithApprenticeshipId(0);

        var response = await fixtures.GetResponse(new GetChangeHistoryQuery() { ApprenticeshipId = fixtures.ApprenticeshipId });

        response.ChangeHistory.Should().BeEmpty();
    }

    public class GetChangeHistoryHandlerTestFixture
    {
        private readonly Fixture _autoFixture;
        public long ApprenticeshipId { get; set; }

        public GetChangeHistoryHandlerTestFixture()
        {
            _autoFixture = new Fixture();
            SeedChangeHistory = _autoFixture.Create<List<LearningChangeHistory>>();
            ApprenticeshipId = _autoFixture.Create<long>();

            SeedChangeHistory.ForEach(x => x.ApprenticeshipId = ApprenticeshipId);
        }

        public List<LearningChangeHistory> SeedChangeHistory { get; }

        public Task<GetChangeHistoryQueryResult> GetResponse(GetChangeHistoryQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetChangeHistoryQueryHandler(lazy);

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

        public GetChangeHistoryHandlerTestFixture GenerateChangeHistoryWithApprenticeshipId(long apprenticeshipId)
        {
            SeedChangeHistory.ForEach(x => x.ApprenticeshipId = apprenticeshipId);
            return this;
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.LearningChangeHistory.AddRange(SeedChangeHistory);

            dbContext.SaveChanges(true);
        }
    }
}