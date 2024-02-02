using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountLegalEntity
{
    [TestFixture]
    public class GetAccountLegalEntityHandlerTests
    {
        [TestCase(123,456,987, ApprenticeshipEmployerType.Levy)]
        [TestCase(1234, 4567, 9870, ApprenticeshipEmployerType.NonLevy)]
        public async Task Handle_WithSpecifiedId_ShouldSetIsValidCorrectly(long accountId, long accountLegalEntityId, long maLegalEntityId, ApprenticeshipEmployerType levyStatus)
        {
            // arrange
            var fixtures = new GetEmployerHandlerTestFixtures()
                .AddAccountWithLegalEntities(accountId, "Account123", accountLegalEntityId, maLegalEntityId, "LegalEntity456", levyStatus);

            // act
            var response = await fixtures.GetResponse(new GetAccountLegalEntityQuery {AccountLegalEntityId = accountLegalEntityId });

            // Assert
            Assert.That(response, Is.Not.Null);
            Assert.That(response.AccountId, Is.EqualTo(accountId));
            Assert.That(response.MaLegalEntityId, Is.EqualTo(maLegalEntityId));
            Assert.That(response.AccountName, Is.EqualTo("Account123"));
            Assert.That(response.LevyStatus, Is.EqualTo(levyStatus));
        }
    }

    public class GetEmployerHandlerTestFixtures
    {
        public GetEmployerHandlerTestFixtures()
        {
            HandlerMock = new Mock<IRequestHandler<GetAccountLegalEntityQuery, GetAccountLegalEntityQueryResult>>();    
            ValidatorMock = new Mock<IValidator<GetAccountLegalEntityQuery>>();
            SeedAccounts = new List<Account>();
        }

        public Mock<IRequestHandler<GetAccountLegalEntityQuery, GetAccountLegalEntityQueryResult>> HandlerMock { get; set; }

        public IRequestHandler<GetAccountLegalEntityQuery, GetAccountLegalEntityQueryResult> Handler => HandlerMock.Object;

        public Mock<IValidator<GetAccountLegalEntityQuery>> ValidatorMock { get; set; }
        public IValidator<GetAccountLegalEntityQuery> Validator => ValidatorMock.Object;

        public List<Account> SeedAccounts { get; }

        public GetEmployerHandlerTestFixtures AddAccountWithLegalEntities(long accountId, string accountName,
            long accountLegalEntityId, long maLegalEntityId, string name, ApprenticeshipEmployerType levyStatus)
        {
            var account = new Account(accountId, "PRI123", "PUB123", accountName, DateTime.Now) { LevyStatus = levyStatus };

            account.AddAccountLegalEntity(accountLegalEntityId, maLegalEntityId, "ABC456", "PUB456", 
                name, OrganisationType.Charities, "My address", DateTime.Now);

            SeedAccounts.Add(account);

            return this;
        }

        public Task<GetAccountLegalEntityQueryResult> GetResponse(GetAccountLegalEntityQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetAccountLegalEntityQueryHandler(lazy);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            return RunWithConnection(connection =>
            {
                var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                    .Options;

                using var dbContext = new ProviderCommitmentsDbContext(options);
                dbContext.Database.EnsureCreated();
                SeedData(dbContext);
                return action(dbContext);
            });
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Accounts.AddRange(SeedAccounts);
            dbContext.AccountLegalEntities.AddRange(SeedAccounts.SelectMany(ac => ac.AccountLegalEntities));
            dbContext.SaveChanges(true);
        }

        private static Task<T> RunWithConnection<T>(Func<DbConnection, Task<T>> action)
        {
            using var connection = new SQLiteConnection("DataSource=:memory:");
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
}
