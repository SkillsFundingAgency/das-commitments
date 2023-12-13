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
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddEmptyDraftCohortWithEmployer(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Cohorts.Length, Is.EqualTo(fixtures.SeedCohorts.Count));
            Assert.That(response.Cohorts[0].AccountId, Is.EqualTo(fixtures.AccountId));
            Assert.That(response.Cohorts[0].ProviderId, Is.EqualTo(fixtures.SeedCohorts[0].ProviderId));
            Assert.That(response.Cohorts[0].CohortId, Is.EqualTo(fixtures.SeedCohorts[0].Id));
            Assert.That(response.Cohorts[0].CreatedOn, Is.EqualTo(fixtures.SeedCohorts[0].CreatedOn));
        }

        [Test]
        public async Task Handle_WithAccountId_ShouldReturnEmptyMessagesAsNothingSent()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddEmptyDraftCohortWithEmployer(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.That(response.Cohorts[0].LatestMessageFromEmployer, Is.Null);
            Assert.That(response.Cohorts[0].LatestMessageFromProvider, Is.Null);
        }

        [Test]
        public async Task Handle_WithAccountIdWithNoCohorts_ShouldReturnEmptyList()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddEmptyDraftCohortWithEmployer(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.NonMatchingId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithAccountIdWithApprovedCohort_ShouldReturnEmptyList()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddCohortForEmployerApprovedByBoth(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithAccountIdWithTransferApprovedCohort_ShouldReturnEmptyList()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddTransferCohortForEmployerAndApprovedByAll(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithAccountIdWithTransferRejectedCohort_ShouldReturnEmptyList()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddTransferCohortForEmployerAndRejectedByAll(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithAccountId_CohortWithTransferSender_ShouldReturnTransferSenderDetails()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddCohortWithTransferSender(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(1));
            Assert.That(response.Cohorts[0].TransferSenderId, Is.EqualTo(fixtures.TransferSenderId));
            Assert.That(response.Cohorts[0].TransferSenderName, Is.EqualTo("TransferSender"));
        }

        [Test]
        public async Task Handle_WithAccountIdWithMixedCohorts_ShouldReturn2CohortsAndExcludeApprovedAndNonMatchingCohorts()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(fixtures.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(fixtures.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(fixtures.NonMatchingId)
                .AddCohortForEmployerApprovedByBoth(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(2));
            Assert.That(response.Cohorts[0].LatestMessageFromEmployer.Text, Is.EqualTo("EmployerLast"));
            Assert.That(response.Cohorts[0].LatestMessageFromProvider.Text, Is.EqualTo("ProviderLast"));
            Assert.That(response.Cohorts[0].NumberOfDraftApprentices, Is.EqualTo(2));
            Assert.That(response.Cohorts[1].LatestMessageFromEmployer.Text, Is.EqualTo("EmployerLast"));
            Assert.That(response.Cohorts[1].LatestMessageFromProvider.Text, Is.EqualTo("ProviderLast"));
            Assert.That(response.Cohorts[1].NumberOfDraftApprentices, Is.EqualTo(2));
        }

        [Test]
        public async Task Handle_WithProviderId_ShouldReturnDraftUnapprovedCohortsForThatProvider()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddEmptyDraftCohortWithEmployer(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(null, fixtures.ProviderId));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(fixtures.SeedCohorts.Count));
            Assert.That(response.Cohorts[0].AccountId, Is.EqualTo(fixtures.AccountId));
            Assert.That(response.Cohorts[0].ProviderId, Is.EqualTo(fixtures.SeedCohorts[0].ProviderId));
            Assert.That(response.Cohorts[0].CohortId, Is.EqualTo(fixtures.SeedCohorts[0].Id));
            Assert.That(response.Cohorts[0].CreatedOn, Is.EqualTo(fixtures.SeedCohorts[0].CreatedOn));
        }

        [Test]
        public async Task Handle_WithProviderIdWithNoCohorts_ShouldReturnEmptyList()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddEmptyDraftCohortWithEmployer(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(null, fixtures.NonMatchingId));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithProviderIdWithApprovedCohort_ShouldReturnEmptyList()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddCohortForEmployerApprovedByBoth(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(null, fixtures.ProviderId));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Handle_WithProviderIdWithMixedCohorts_ShouldReturn3CohortsForProviderIdAndExcludeApprovedCohorts()
        {
            var fixtures = new GetCohortsHandlerTestFixtures();
            fixtures.AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(fixtures.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(fixtures.AccountId)
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices(fixtures.NonMatchingId)
                .AddCohortForEmployerApprovedByBoth(fixtures.AccountId);

            var response = await fixtures.GetResponse(new GetCohortsQuery(fixtures.AccountId, null));

            Assert.IsNotNull(response);
            Assert.That(response.Cohorts.Length, Is.EqualTo(2));
            Assert.That(response.Cohorts[0].LatestMessageFromEmployer.Text, Is.EqualTo("EmployerLast"));
            Assert.That(response.Cohorts[0].LatestMessageFromProvider.Text, Is.EqualTo("ProviderLast"));
            Assert.That(response.Cohorts[0].NumberOfDraftApprentices, Is.EqualTo(2));
            Assert.That(response.Cohorts[1].LatestMessageFromEmployer.Text, Is.EqualTo("EmployerLast"));
            Assert.That(response.Cohorts[1].LatestMessageFromProvider.Text, Is.EqualTo("ProviderLast"));
            Assert.That(response.Cohorts[1].NumberOfDraftApprentices, Is.EqualTo(2));
        }
    }

    public class GetCohortsHandlerTestFixtures
    {
        private readonly Fixture _autoFixture;

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

        private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            using var dbContext = new ProviderCommitmentsDbContext(options);
            dbContext.Database.EnsureCreated();
            SeedData(dbContext);
            return action(dbContext);
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
