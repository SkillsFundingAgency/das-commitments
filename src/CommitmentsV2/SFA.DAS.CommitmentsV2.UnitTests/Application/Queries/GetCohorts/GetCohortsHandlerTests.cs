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
            f.AddEmptyDraftCohortWithEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(f.SeedCohorts.Count, response.Cohorts.Length);
            Assert.AreEqual(f.AccountId, response.Cohorts[0].AccountId);
            Assert.AreEqual(f.SeedCohorts[0].ProviderId, response.Cohorts[0].ProviderId);
            Assert.AreEqual(f.SeedCohorts[0].Id, response.Cohorts[0].CohortId);
            Assert.AreEqual(f.SeedCohorts[0].CreatedOn, response.Cohorts[0].CreatedOn);
        }

        [Test]
        public async Task Handle_WithAccountId_ShouldReturnEmptyMessagesAsNothingSent()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortWithEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNull(response.Cohorts[0].LatestMessageFromEmployer);
            Assert.IsNull(response.Cohorts[0].LatestMessageFromProvider);
        }

        [Test]
        public async Task Handle_WithAccountIdWithNoCohorts_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortWithEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.NonMatchingId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Cohorts.Length);
        }

        [Test]
        public async Task Handle_WithAccountIdWithApprovedCohort_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddCohortForEmployerApprovedByBoth(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Cohorts.Length);
        }

        [Test]
        public async Task Handle_WithAccountIdWithTransferApprovedCohort_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddTransferCohortForEmployerAndApprovedByAll(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Cohorts.Length);
        }

        [Test]
        public async Task Handle_WithAccountIdWithTransferRejectedCohort_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddTransferCohortForEmployerAndRejectedByAll(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Cohorts.Length);
        }

        [Test]
        public async Task Handle_WithAccountId_CohortWithTransferSender_ShouldReturnTransferSenderDetails()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddCohortWithTransferSender(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.Cohorts.Length);
            Assert.AreEqual(f.TransferSenderId, response.Cohorts[0].TransferSenderId);
            Assert.AreEqual("TransferSender", response.Cohorts[0].TransferSenderName);
        }

        [Test]
        public async Task Handle_WithAccountIdWithMixedCohorts_ShouldReturn2CohortsAndExcludeApprovedAndNonMatchingCohorts()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.NonMatchingId)
                .AddCohortForEmployerApprovedByBoth(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Cohorts.Length);
            Assert.AreEqual("EmployerLast", response.Cohorts[0].LatestMessageFromEmployer.Text);
            Assert.AreEqual("ProviderLast", response.Cohorts[0].LatestMessageFromProvider.Text);
            Assert.AreEqual(2, response.Cohorts[0].NumberOfDraftApprentices);
            Assert.AreEqual("EmployerLast", response.Cohorts[1].LatestMessageFromEmployer.Text);
            Assert.AreEqual("ProviderLast", response.Cohorts[1].LatestMessageFromProvider.Text);
            Assert.AreEqual(2, response.Cohorts[1].NumberOfDraftApprentices);
        }

        [Test]
        public async Task Handle_WithProviderId_ShouldReturnDraftUnapprovedCohortsForThatProvider()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortWithEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(null, f.ProviderId));

            Assert.IsNotNull(response);
            Assert.AreEqual(f.SeedCohorts.Count, response.Cohorts.Length);
            Assert.AreEqual(f.AccountId, response.Cohorts[0].AccountId);
            Assert.AreEqual(f.SeedCohorts[0].ProviderId, response.Cohorts[0].ProviderId);
            Assert.AreEqual(f.SeedCohorts[0].Id, response.Cohorts[0].CohortId);
            Assert.AreEqual(f.SeedCohorts[0].CreatedOn, response.Cohorts[0].CreatedOn);
        }

        [Test]
        public async Task Handle_WithProviderIdWithNoCohorts_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddEmptyDraftCohortWithEmployer(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(null, f.NonMatchingId));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Cohorts.Length);
        }

        [Test]
        public async Task Handle_WithProviderIdWithApprovedCohort_ShouldReturnEmptyList()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddCohortForEmployerApprovedByBoth(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(null, f.ProviderId));

            Assert.IsNotNull(response);
            Assert.AreEqual(0, response.Cohorts.Length);
        }

        [Test]
        public async Task Handle_WithProviderIdWithMixedCohorts_ShouldReturn3CohortsForProviderIdAndExcludeApprovedCohorts()
        {
            var f = new GetCohortsHandlerTestFixtures();
            f.AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(f.NonMatchingId)
                .AddCohortForEmployerApprovedByBoth(f.AccountId);

            var response = await f.GetResponse(new GetCohortsQuery(f.AccountId, null));

            Assert.IsNotNull(response);
            Assert.AreEqual(2, response.Cohorts.Length);
            Assert.AreEqual("EmployerLast", response.Cohorts[0].LatestMessageFromEmployer.Text);
            Assert.AreEqual("ProviderLast", response.Cohorts[0].LatestMessageFromProvider.Text);
            Assert.AreEqual(2, response.Cohorts[0].NumberOfDraftApprentices);
            Assert.AreEqual("EmployerLast", response.Cohorts[1].LatestMessageFromEmployer.Text);
            Assert.AreEqual("ProviderLast", response.Cohorts[1].LatestMessageFromProvider.Text);
            Assert.AreEqual(2, response.Cohorts[1].NumberOfDraftApprentices);
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
            ProviderId = _autoFixture.Create<long>();
            TransferSenderId = AccountId + 1;

            TransferSender = new Account(TransferSenderId, "", "", "TransferSender", DateTime.UtcNow);
            Account = new Account(AccountId,"","","TEST", DateTime.UtcNow);
            AccountLegalEntity = new AccountLegalEntity(Account, 1,1, "", "","TEST", OrganisationType.Charities, "", DateTime.UtcNow);
            Provider = new Provider(ProviderId, "TEST PROVIDER", DateTime.UtcNow, DateTime.UtcNow);
        }

        public long AccountId { get; }
        public long ProviderId { get; }
        public long TransferSenderId { get; set; }
        public long NonMatchingId => AccountId + 100;
        public List<Cohort> SeedCohorts { get; }
        public List<Account> SeedAccounts { get; }
        public Account Account { get; }
        public Account TransferSender { get; }
        public AccountLegalEntity AccountLegalEntity { get; }
        public Provider Provider { get; set; }


        public Task<GetCohortsResult> GetResponse(GetCohortsQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetCohortsHandler(lazy, Mock.Of<ILogger<GetCohortsHandler>>());

                return handler.Handle(query, CancellationToken.None);
            });
        }

        public GetCohortsHandlerTestFixtures AddEmptyDraftCohortWithEmployer(long? accountId)
        {
            var cohort = _autoFixture.Build<Cohort>().With(o=>o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Neither)
                .With(o => o.IsDeleted, false)
                .With(o => o.AccountLegalEntity, AccountLegalEntity)
                .With(o => o.Provider, Provider)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferSender)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
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
                .With(o => o.AccountLegalEntity, AccountLegalEntity)
                .With(o => o.Provider, Provider)
                .Without(o => o.TransferSender)
                .Without(o => o.TransferSenderId)
                .Without(o => o.TransferApprovalStatus)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
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
                .With(o => o.AccountLegalEntity, AccountLegalEntity)
                .With(o => o.Provider, Provider)
                .With(o => o.TransferSender, TransferSender)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
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
                .With(o => o.AccountLegalEntity, AccountLegalEntity)
                .With(o => o.Provider, Provider)
                .With(o => o.TransferSender, TransferSender)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddTransferCohortForEmployerAndRejectedByAll(long accountId)
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.TransferSenderId, TransferSenderId)
                .With(o => o.EditStatus, EditStatus.Both)
                .With(o => o.TransferApprovalStatus, TransferApprovalStatus.Rejected)
                .With(o => o.IsDeleted, false)
                .With(o => o.AccountLegalEntity, AccountLegalEntity)
                .With(o => o.Provider, Provider)
                .With(o => o.TransferSender, TransferSender)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.Messages)
                .Create();

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetCohortsHandlerTestFixtures AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(long accountId)
        {

            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, accountId)
                .With(o => o.EditStatus, EditStatus.Neither)
                .With(o => o.IsDeleted, false)
                .With(o => o.AccountLegalEntity, AccountLegalEntity)
                .With(o => o.Provider, Provider)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.TransferSender)
                .Without(o => o.Messages)
                .Create();

            var apprenticeships = _autoFixture.Build<DraftApprenticeship>()
                .Without(a => a.Cohort)
                .Without(a => a.EpaOrg)
                .Without(a => a.ApprenticeshipUpdate)
                .Without(a => a.PreviousApprenticeship)
                .Without(s => s.ApprenticeshipConfirmationStatus)
                .CreateMany(2);

            foreach (var app in apprenticeships)
            {
                cohort.Apprenticeships.Add(app);
            }

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
