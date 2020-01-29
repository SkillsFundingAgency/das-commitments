using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Moq;
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
        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        public async Task Handle_Should_Return_Expected_HasCohorts_Value(int cohorts, bool expectedHasCohorts)
        {
            var fixture = new GetAccountSummaryHandlerTestsFixture();
            fixture
                .AddCohorts(cohorts)
                .AddApprentices(10)
                .AddNoise();

            var response = await fixture.GetResponse();

            Assert.AreEqual(expectedHasCohorts, response.HasCohorts);
        }

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(10, true)]
        public async Task Handle_Should_Return_Expected_HasApprentices_Value(int apprentices,
            bool expectedHasApprentices)
        {
            var fixture = new GetAccountSummaryHandlerTestsFixture();
            fixture
                .AddCohorts(10)
                .AddApprentices(apprentices)
                .AddNoise();

            var response = await fixture.GetResponse();

            Assert.AreEqual(expectedHasApprentices, response.HasApprenticeships);
        }
    }

    public class GetAccountSummaryHandlerTestsFixture
    {
        public GetAccountSummaryHandlerTestsFixture()
        {
            var autoFixture = new Fixture();

            HandlerMock = new Mock<IRequestHandler<GetAccountSummaryQuery, GetAccountSummaryQueryResult>>();
            ValidatorMock = new Mock<IValidator<GetAccountSummaryQuery>>();

            SeedApprenticeships = new List<Apprenticeship>();
            SeedCohorts = new List<Cohort>();

            EmployerAccountId = autoFixture.Create<long>();
        }

        public Mock<IRequestHandler<GetAccountSummaryQuery, GetAccountSummaryQueryResult>> HandlerMock { get; set; }

        public Mock<IValidator<GetAccountSummaryQuery>> ValidatorMock { get; set; }
        public long EmployerAccountId { get; }

        public List<Cohort> SeedCohorts { get; set; }
        public List<Apprenticeship> SeedApprenticeships { get; set; }

        public GetAccountSummaryHandlerTestsFixture AddCohorts(int numberOfCohorts)
        {
            AddCohortsForEmployerAccount(EmployerAccountId, numberOfCohorts);
            return this;
        }

        private void AddCohortsForEmployerAccount(long employerAccountId, int numberOfCohorts)
        {
            for (var i = 0; i < numberOfCohorts; i++)
            {
                var cohort = new Cohort
                {
                    EmployerAccountId = employerAccountId,
                    EditStatus = EditStatus.EmployerOnly,
                    Originator = Originator.Employer
                };

                SeedCohorts.Add(cohort);
            }
        }

        public GetAccountSummaryHandlerTestsFixture AddApprentices(int numberOfApprentices)
        {
            AddApprenticesForEmployerAccount(EmployerAccountId, numberOfApprentices);
            return this;
        }

        private void AddApprenticesForEmployerAccount(long employerAccountId, int numberOfApprentices)
        {
            for (var i = 0; i < numberOfApprentices; i++)
            {
                var approvedCohort = new Cohort
                {
                    EmployerAccountId = employerAccountId,
                    EditStatus = EditStatus.Both,
                    Originator = Originator.Employer
                };

                var apprenticeship = new Apprenticeship
                {
                    Cohort = approvedCohort
                };

                SeedCohorts.Add(approvedCohort);
                SeedApprenticeships.Add(apprenticeship);
            }
        }

        public GetAccountSummaryHandlerTestsFixture AddNoise()
        {
            AddCohortsForEmployerAccount(EmployerAccountId + 1, 10);
            AddApprenticesForEmployerAccount(EmployerAccountId + 1, 10);
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
                .UseInMemoryDatabase("SFA.DAS.Commitments.Database")
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
            dbContext.Cohorts.AddRange(SeedCohorts);
            dbContext.Apprenticeships.AddRange(SeedApprenticeships);
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