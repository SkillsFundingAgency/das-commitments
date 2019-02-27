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
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Queries.GetEmployer;

namespace SFA.DAS.CommitmentsV2.UnitTests.Queries.GetEmployer
{
    [TestFixture]
    public class GetEmployerHandlerTests
    {
        [Test]
        public async Task Validate_WithSpecifiedId_ShouldSetIsValidCorrectly()
        {
            const long accountLegalEntityId = 456;

            // arrange
            var fixtures = new GetEmployerHandlerTestFixtures()
                .AddAccountWithLegalEntities(123, "Account123", accountLegalEntityId, "LegalEntity456");

            // act
            var response = await fixtures.GetResponse(new GetEmployerRequest {AccountLegalEntityId = accountLegalEntityId });

            // Assert
            Assert.IsNotNull(response);
        }
    }


    public class GetEmployerHandlerTestFixtures
    {
        public GetEmployerHandlerTestFixtures()
        {
            HandlerMock = new Mock<IRequestHandler<GetEmployerRequest, GetEmployerResponse>>();    
            ValidatorMock = new Mock<IValidator<GetEmployerRequest>>();
            SeedAccounts = new List<Account>();
        }

        public Mock<IRequestHandler<GetEmployerRequest, GetEmployerResponse>> HandlerMock { get; set; }

        public IRequestHandler<GetEmployerRequest, GetEmployerResponse> Handler => HandlerMock.Object;

        public Mock<IValidator<GetEmployerRequest>> ValidatorMock { get; set; }
        public IValidator<GetEmployerRequest> Validator => ValidatorMock.Object;

        public List<Account> SeedAccounts { get; }

        public GetEmployerHandlerTestFixtures AddAccountWithLegalEntities(long accountId, string accountName, long accountLegalEntityId, string name)
        {
            var account = new Account(accountId, "PRI123", "PUB123", accountName, DateTime.Now);

            account.AddAccountLegalEntity(accountLegalEntityId, "PUB456", name, DateTime.Now);

            SeedAccounts.Add(account);

            return this;
        }

        public Task<GetEmployerResponse> GetResponse(GetEmployerRequest request)
        {
            return RunWithDbContext(dbContext =>
            {
                var handler = new GetEmployerHandler(dbContext);

                return handler.Handle(request, CancellationToken.None);
            });
        }

        public Task<T> RunWithDbContext<T>(Func<AccountsDbContext, Task<T>> action)
        {
            return RunWithConnection(connection =>
            {
                var options = new DbContextOptionsBuilder<AccountsDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var dbContext = new AccountsDbContext(options))
                {
                    dbContext.Database.EnsureCreated();
                    SeedData(dbContext);
                    return action(dbContext);
                }
            });
        }

        private void SeedData(AccountsDbContext dbContext)
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
    }
}
