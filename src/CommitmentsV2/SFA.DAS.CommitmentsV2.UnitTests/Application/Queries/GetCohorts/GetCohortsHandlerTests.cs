using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetCohorts
{
    [TestFixture]
    public class GetCohortsHandlerTests
    {
        [Test]
        public async Task Handle_WithAccountId_ShouldReturnDraftUnapprovedCohortsForThatEmployer()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortForEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId));

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Cohorts.Length, f.SeedCohorts.Count);
            Assert.AreEqual(response.Cohorts[0].AccountId, f.AccountId);
            Assert.AreEqual(response.Cohorts[0].LegalEntityName, f.SeedCohorts[0].LegalEntityName);
            Assert.AreEqual(response.Cohorts[0].ProviderId, f.SeedCohorts[0].ProviderId);
            Assert.AreEqual(response.Cohorts[0].ProviderName, f.SeedCohorts[0].ProviderName);
            Assert.AreEqual(response.Cohorts[0].CohortId, f.SeedCohorts[0].Id);
        }

        [Test]
        public async Task Handle_WithAccountIdWithNoCohorts_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortForEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.NonMatchingAccountId));

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Cohorts.Length, 0);
        }

        [Test]
        public async Task Handle_WithAccountIdWithApprovedCohort_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddCohortForEmployerApprovedByBoth(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId));

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Cohorts.Length, 0);
        }

        [Test]
        public async Task Handle_WithAccountIdWithTransferApprovedCohort_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddTransferCohortForEmployerAndApprovedByAll(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId));

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Cohorts.Length, 0);
        }

        [Test]
        public async Task Handle_WithAccountIdWithMixedCohorts_ShouldReturn2CohortsAndExcludeApprovedAndNonMatchingCohorts()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.NonMatchingAccountId)
                .AddCohortForEmployerApprovedByBoth(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId));

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Cohorts.Length, 2);
            Assert.AreEqual(response.Cohorts[0].LastMessageFromEmployer, "EmployerLast");
            Assert.AreEqual(response.Cohorts[0].LastMessageFromProvider, "ProviderLast");
            Assert.AreEqual(response.Cohorts[0].NumberOfDraftApprentices, 2);
            Assert.AreEqual(response.Cohorts[1].LastMessageFromEmployer, "EmployerLast");
            Assert.AreEqual(response.Cohorts[1].LastMessageFromProvider, "ProviderLast");
            Assert.AreEqual(response.Cohorts[1].NumberOfDraftApprentices, 2);
        }
    }

    public class GetCohortsHandlerTestFixtures
    {
        private Fixture _autoFixture;

        public GetCohortsHandlerTestFixtures()
        {
            SeedCohorts = new List<Cohort>();
            _autoFixture = new Fixture();
            AccountId = _autoFixture.Create<long>();
        }

        public long AccountId { get; }
        public long NonMatchingAccountId => AccountId + 100;
        public List<Cohort> SeedCohorts { get; }

        public Task<GetCohortsResult> GetResponse(GetCohortsQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortsHandler(lazy);

                return handler.Handle(query, CancellationToken.None);
            });
        }

        public GetCohortsHandlerTestFixtures AddEmptyDraftCohortForEmployer(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>().With(o=>o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Neither)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages).Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddCohortForEmployerApprovedByBoth(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Both)
                .Without(o => o.TransferSenderId)
                .Without(o => o.TransferApprovalStatus)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages).Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddTransferCohortForEmployerAndApprovedByAll(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Both)
                .With(o => o.TransferApprovalStatus, TransferApprovalStatus.Approved)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages).Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(long accountId)
        {

            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Neither)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages).Create();

            cohort.Apprenticeships.Add(new DraftApprenticeship());
            cohort.Apprenticeships.Add(new DraftApprenticeship());

            cohort.Messages.Add(new Message(cohort, Party.Employer, "XXX", "NotLast"));
            cohort.Messages.Add(new Message(cohort, Party.Provider, "XXX", "NotLast"));

            cohort.Messages.Add(new Message(cohort, Party.Employer, "XXX", "EmployerLast"));
            cohort.Messages.Add(new Message(cohort, Party.Provider, "XXX", "ProviderLast"));

            SeedCohorts.Add(cohort);
            return this;
        }

        public Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase("SFA.DAS.Commitments.Database")
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
            dbContext.SaveChanges(true);
        }
    }
}
