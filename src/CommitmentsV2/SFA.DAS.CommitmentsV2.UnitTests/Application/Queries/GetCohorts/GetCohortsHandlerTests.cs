using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetCohorts;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using Message = SFA.DAS.CommitmentsV2.Models.Message;

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
            Assert.AreEqual(response.Cohorts[0].CreatedOn, f.SeedCohorts[0].CreatedOn);
        }

        [Test]
        public async Task Handle_WithAccountId_ShouldReturnEmptyMessagesAsNothingSent()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortForEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId));

            Assert.IsNull(response.Cohorts[0].LatestMessageFromEmployer);
            Assert.IsNull(response.Cohorts[0].LatestMessageFromProvider);
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
        public async Task Handle_WithAccountId_CohortWithTransferSender_ShouldReturnTransferSenderDetails()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddCohortWithTransferSender(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId));

            Assert.IsNotNull(response);
            Assert.AreEqual(response.Cohorts.Length, 1);
            Assert.AreEqual(response.Cohorts[0].TransferSenderId, f.TransferSenderId);
            Assert.AreEqual(response.Cohorts[0].TransferSenderName, "TransferSender");
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
            Assert.AreEqual(response.Cohorts[0].LatestMessageFromEmployer.Text, "EmployerLast");
            Assert.AreEqual(response.Cohorts[0].LatestMessageFromProvider.Text, "ProviderLast");
            Assert.AreEqual(response.Cohorts[0].NumberOfDraftApprentices, 2);
            Assert.AreEqual(response.Cohorts[1].LatestMessageFromEmployer.Text, "EmployerLast");
            Assert.AreEqual(response.Cohorts[1].LatestMessageFromProvider.Text, "ProviderLast");
            Assert.AreEqual(response.Cohorts[1].NumberOfDraftApprentices, 2);
        }
    }

    public class GetCohortsHandlerTestFixtures
    {
        private Fixture _autoFixture;

        public GetCohortsHandlerTestFixtures()
        {
            SeedCohorts = new List<Cohort>();
            SeedAccounts = new List<Account>();
            _autoFixture = new Fixture();
            AccountId = _autoFixture.Create<long>();
            TransferSenderId = _autoFixture.Create<long>();
        }

        public long AccountId { get; }
        public long TransferSenderId { get; set; }
        public long NonMatchingAccountId => AccountId + 100;
        public List<Cohort> SeedCohorts { get; }
        public List<Account> SeedAccounts { get; }



        public Task<GetCohortsResult> GetResponse(GetCohortsQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortsHandler(lazy, Mock.Of<ILogger<GetCohortsHandler>>());

                return handler.Handle(query, CancellationToken.None);
            });
        }

        public GetCohortsHandlerTestFixtures AddEmptyDraftCohortForEmployer(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>().With(o=>o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Neither)
                .With(o => o.IsDeleted, false)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Without(o => o.AccountLegalEntity)
                .Without(o => o.Provider)
                .Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddCohortForEmployerApprovedByBoth(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Both)
                .With(o => o.IsDeleted, false)
                .Without(o => o.TransferSenderId)
                .Without(o => o.TransferApprovalStatus)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Without(o => o.AccountLegalEntity)
                .Without(o => o.Provider)
                .Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddTransferCohortForEmployerAndApprovedByAll(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Both)
                .With(o => o.TransferApprovalStatus, TransferApprovalStatus.Approved)
                .With(o => o.IsDeleted, false)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Without(o => o.AccountLegalEntity)
                .Without(o => o.Provider)
                .Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddCohortWithTransferSender(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.TransferSenderId, TransferSenderId)
                .With(o => o.EditStatus, EditStatus.Both)
                .With(o => o.TransferApprovalStatus, TransferApprovalStatus.Pending)
                .With(o => o.IsDeleted, false)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Without(o => o.AccountLegalEntity)
                .Without(o => o.Provider)
                .Create();

            var account = new Account(TransferSenderId, "hashedId", "publicHashedId", "TransferSender", DateTime.Now);

            SeedAccounts.Add(account);
            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(long accountId)
        {

            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Neither)
                .With(o => o.IsDeleted, false)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Without(o => o.AccountLegalEntity)
                .Without(o => o.Provider)
                .Create();

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
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
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
            
            if (SeedAccounts.Count > 0)
            {
                dbContext.Accounts.AddRange(SeedAccounts);
            }

            dbContext.SaveChanges(true);
        }
    }
}
