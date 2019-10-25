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
using Microsoft.Extensions.Logging.Console;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetAccountLegalEntity
{
    [TestFixture]
    public class GetAccountLegalEntityHandlerTests
    {
        [Test]
        public async Task Handle_WithSpecifiedId_ShouldSetIsValidCorrectly()
        {
            const long accountId = 123;
            const long accountLegalEntityId = 456;
            const long maLegalEntityId = 987;

            // arrange
            var fixtures = new GetEmployerHandlerTestFixtures()
                .AddAccountWithLegalEntities(accountId, "Account123", accountLegalEntityId, maLegalEntityId, "LegalEntity456");

            // act
            var response = await fixtures.GetResponse(new GetAccountLegalEntityRequest {AccountLegalEntityId = accountLegalEntityId });

            // Assert
            Assert.IsNotNull(response);
            Assert.AreEqual(response.AccountId, accountId);
            Assert.AreEqual(response.AccountName, "Account123");
        }
    }


    public class GetEmployerHandlerTestFixtures
    {
        public GetEmployerHandlerTestFixtures()
        {
            HandlerMock = new Mock<IRequestHandler<GetAccountLegalEntityRequest, GetAccountLegalEntityResponse>>();    
            ValidatorMock = new Mock<IValidator<GetAccountLegalEntityRequest>>();
            SeedAccounts = new List<Account>();
        }

        public Mock<IRequestHandler<GetAccountLegalEntityRequest, GetAccountLegalEntityResponse>> HandlerMock { get; set; }

        public IRequestHandler<GetAccountLegalEntityRequest, GetAccountLegalEntityResponse> Handler => HandlerMock.Object;

        public Mock<IValidator<GetAccountLegalEntityRequest>> ValidatorMock { get; set; }
        public IValidator<GetAccountLegalEntityRequest> Validator => ValidatorMock.Object;

        public List<Account> SeedAccounts { get; }

        public GetEmployerHandlerTestFixtures AddAccountWithLegalEntities(long accountId, string accountName,
            long accountLegalEntityId, long maLegalEntityId, string name)
        {
            var account = new Account(accountId, "PRI123", "PUB123", accountName, DateTime.Now);

            account.AddAccountLegalEntity(accountLegalEntityId, maLegalEntityId, "ABC456", "PUB456", 
                name, OrganisationType.Charities, "My address", DateTime.Now);

            SeedAccounts.Add(account);

            return this;
        }

        public Task<GetAccountLegalEntityResponse> GetResponse(GetAccountLegalEntityRequest request)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetAccountLegalEntityHandler(lazy);

                return handler.Handle(request, CancellationToken.None);
            });
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            return RunWithConnection(connection =>
            {
                var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                    .UseSqlite(connection)
                    .UseLoggerFactory(MyLoggerFactory)
                    .Options;

                using (var dbContext = new ProviderCommitmentsDbContext(options))
                {
                    dbContext.Database.EnsureCreated();
                    SeedData(dbContext);
                    return action(dbContext);
                }
            });
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Accounts.AddRange(SeedAccounts);
            dbContext.AccountLegalEntities.AddRange(SeedAccounts.SelectMany(ac => ac.AccountLegalEntities));
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

        public static readonly LoggerFactory MyLoggerFactory
            = new LoggerFactory(new[]
            {
#pragma warning disable 618
                new ConsoleLoggerProvider((category, level)
#pragma warning restore 618
                    => category == DbLoggerCategory.Database.Command.Name
                       && level == LogLevel.Debug, true)
            });
    }
}
