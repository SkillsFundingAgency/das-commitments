using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeHistoryForEmployer;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetChangeHistoryForEmployer;

public class GetChangeHistoryForEmployerTests
{
    [Test]
    public async Task Handle_WithAccountId_ShouldReturnAllChangeHistory()
    {
        var fixtures = new GetChangeHistoryForEmployerQueryHandlerTestFixture();

        var response = await fixtures.GetResponse(new() { AccountId = fixtures.AccountId });

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
            ProviderName = x.ProviderName,
        }));
    }

    [Test]
    public async Task Handle_WithNoMatchingAccountId_ShouldReturnEmptyChangeHistory()
    {
        var fixtures = new GetChangeHistoryForEmployerQueryHandlerTestFixture().GenerateChangeHistoryWithAccountId(0);

        var response = await fixtures.GetResponse(new GetChangeHistoryForEmployerQuery() { AccountId = fixtures.AccountId });
        response.ChangeHistory.Should().BeEmpty();
    }

    public class GetChangeHistoryForEmployerQueryHandlerTestFixture
    {
        private readonly Fixture _autoFixture;
        public long AccountId { get; set; }

        public GetChangeHistoryForEmployerQueryHandlerTestFixture()
        {
            _autoFixture = new Fixture();
            SeedChangeHistory = _autoFixture.Create<List<LearningChangeHistory>>();
            AccountId = _autoFixture.Create<long>();
            SeedChangeHistory.ForEach(x => x.AccountId = AccountId);
        }

        public List<LearningChangeHistory> SeedChangeHistory { get; }

        public Task<GetChangeHistoryForEmployerQueryResult> GetResponse(GetChangeHistoryForEmployerQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetChangeHistoryForEmployerQueryHandler(lazy);

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

        public GetChangeHistoryForEmployerQueryHandlerTestFixture GenerateChangeHistoryWithAccountId(long accountId)
        {
            SeedChangeHistory.ForEach(x => x.AccountId = accountId);
            return this;
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.LearningChangeHistory.AddRange(SeedChangeHistory);

            dbContext.SaveChanges(true);
        }
    }
}