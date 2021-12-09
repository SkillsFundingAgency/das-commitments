using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
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

            Assert.AreEqual(ApprenticeshipEmployerType.NonLevy, response.LevyStatus);
        }

        public async Task Handle_Should_Return_LevyStatus_Levy()
        {
            var fixture = new GetAccountSummaryHandlerTestsFixture();
            fixture.AddAccount().SetEmployerLevyStatusToLevy();

            var response = await fixture.GetResponse();

            Assert.AreEqual(ApprenticeshipEmployerType.Levy, response.LevyStatus);
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

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseLoggerFactory(MyLoggerFactory)
                .Options;

            using (var dbContext = new ProviderCommitmentsDbContext(options))
            {
                dbContext.Database.EnsureCreated();
                SeedData(dbContext);
                return action(dbContext);
            }
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Accounts.AddRange(SeedAccounts);
            dbContext.SaveChanges(true);
        }

        public Task<T> RunWithConnection<T>(Func<DbConnection, Task<T>> action)
        {
            using (var connection = new SQLiteConnection("DataSource=:memory:"))
            {
                connection.Open();
                try
                {
                    return action(connection);
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        internal void SetEmployerLevyStatusToLevy()
        {
            var account = SeedAccounts.First(x => x.Id == EmployerAccountId);
            account.LevyStatus = ApprenticeshipEmployerType.Levy;
        }

        public static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
    }
}