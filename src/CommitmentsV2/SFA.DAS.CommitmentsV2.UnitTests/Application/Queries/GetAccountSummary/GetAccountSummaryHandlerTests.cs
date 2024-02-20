using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountSummary;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountSummary
{
    [TestFixture]
    public class GetAccountSummaryHandlerTests
    {
        [Test]
        public async Task Handle_Should_Return_LevyStatus_NonLevy()
        {
            var fixture = new GetAccountSummaryHandlerTestsFixture();
            fixture.AddAccount();

            var response = await fixture.GetResponse();

            Assert.That(response.LevyStatus, Is.EqualTo(ApprenticeshipEmployerType.NonLevy));
        }

        [Test]
        public async Task Handle_Should_Return_LevyStatus_Levy()
        {
            var fixture = new GetAccountSummaryHandlerTestsFixture();
            fixture.AddAccount().SetEmployerLevyStatusToLevy();

            var response = await fixture.GetResponse();

            Assert.That(response.LevyStatus, Is.EqualTo(ApprenticeshipEmployerType.Levy));
        }
    }

    public class GetAccountSummaryHandlerTestsFixture
    {
        public GetAccountSummaryHandlerTestsFixture()
        {
            var autoFixture = new Fixture();
            SeedAccounts = new List<Account>();

            EmployerAccountId = autoFixture.Create<long>();
        }

        public long EmployerAccountId { get; }

        public List<Account> SeedAccounts { get; set; }

        public GetAccountSummaryHandlerTestsFixture AddAccount()
        {
            SeedAccounts.Add(new Account(EmployerAccountId, "XYZ", "ZZZ", "Account1", DateTime.Now));
            return this;

        }

        public Task<GetAccountSummaryQueryResult> GetResponse()
        {
            var request = new GetAccountSummaryQuery
            {
                AccountId = EmployerAccountId
            };

            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetAccountSummaryQueryHandler(lazy);

                return handler.Handle(request, CancellationToken.None);
            });
        }

        private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .UseLoggerFactory(MyLoggerFactory)
                .Options;

            using var dbContext = new ProviderCommitmentsDbContext(options);
            dbContext.Database.EnsureCreated();
            SeedData(dbContext);
            return action(dbContext);
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Accounts.AddRange(SeedAccounts);
            dbContext.SaveChanges(true);
        }
        
        internal void SetEmployerLevyStatusToLevy()
        {
            var account = SeedAccounts.First(x => x.Id == EmployerAccountId);
            account.LevyStatus = ApprenticeshipEmployerType.Levy;
        }

        private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }
}