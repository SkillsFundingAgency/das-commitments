using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Queries.GetProviderCommitmentAgreements;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.ProviderRelationships.Api.Client;
using Message = SFA.DAS.CommitmentsV2.Models.Message;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetProviderCommitmentAgreements
{
    [TestFixture]
    public class GetProviderCommitmentAgreementsHandlerTests
    {

        [Test]
        public async Task Handle_WithApprovedAndUnApprovedCohort_ShouldReturnAllProviderCohortEmployers()
        {
            var testFixture = new GetProviderCommitmentAgreementsHandlerTestFixtures();
            testFixture
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices()
                .AddCohortForEmployerApprovedByBoth();

            var response = await testFixture.GetResponse(new GetProviderCommitmentAgreementQuery(testFixture.Provider.UkPrn));

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Agreements, Has.Count.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(response.Agreements[0].AccountLegalEntityPublicHashedId, Is.EqualTo(testFixture.SeedAccountLegalEntities[0].PublicHashedId));
                Assert.That(response.Agreements[1].AccountLegalEntityPublicHashedId, Is.EqualTo(testFixture.SeedAccountLegalEntities[1].PublicHashedId));
            });

        }

        [Test]
        public async Task Handle_WithEmployersThatHasGivenPermissionToProvider_ShouldReturnAllEmployers()
        {
            var testFixture = new GetProviderCommitmentAgreementsHandlerTestFixtures();
            testFixture.AddEmployersWithPermissions();

            var response = await testFixture.GetResponse(new GetProviderCommitmentAgreementQuery(testFixture.Provider.UkPrn));

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Agreements, Has.Count.EqualTo(3));
            Assert.Multiple(() =>
            {
                Assert.That(response.Agreements[0].AccountLegalEntityPublicHashedId, Is.EqualTo(testFixture.SeedAccountProviderLegalEntitiesDto[0].AccountLegalEntityPublicHashedId));
                Assert.That(response.Agreements[1].AccountLegalEntityPublicHashedId, Is.EqualTo(testFixture.SeedAccountProviderLegalEntitiesDto[1].AccountLegalEntityPublicHashedId));
                Assert.That(response.Agreements[2].AccountLegalEntityPublicHashedId, Is.EqualTo(testFixture.SeedAccountProviderLegalEntitiesDto[2].AccountLegalEntityPublicHashedId));
            });
        }

        [Test]
        public async Task Handle_WithApprovedAndUnApprovedCohortAndEmployersThatHasGivenPermissionToProvider_ShouldReturnAllEmployers()
        {
            var testFixture = new GetProviderCommitmentAgreementsHandlerTestFixtures();
            testFixture
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices()
                .AddCohortForEmployerApprovedByBoth()
                .AddEmployersWithPermissions();

            var response = await testFixture.GetResponse(new GetProviderCommitmentAgreementQuery(testFixture.Provider.UkPrn));

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Agreements, Has.Count.EqualTo(5));
        }

        [Test]
        public async Task Handle_WithApprovedAndUnApprovedCohortAndEmployersThatHasGivenPermissionToProvider_ShouldReturnDistinctEmployers()
        {
            var testFixture = new GetProviderCommitmentAgreementsHandlerTestFixtures();
            testFixture
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices()
                .AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices()
                .AddCohortForEmployerApprovedByBoth()
                .AddEmployersWithPermissions();

            var response = await testFixture.GetResponse(new GetProviderCommitmentAgreementQuery(testFixture.Provider.UkPrn));

            Assert.That(response, Is.Not.Null);
            Assert.That(response.Agreements, Has.Count.EqualTo(5));
        }
    }

    public class GetProviderCommitmentAgreementsHandlerTestFixtures
    {
        private Fixture _autoFixture;
        public List<Cohort> SeedCohorts { get; }
        public List<AccountLegalEntity> SeedAccountLegalEntities { get; }
        public Account Account { get; }
        public Account TransferSender { get; }
        public Provider Provider { get; set; }
        public Mock<IProviderRelationshipsApiClient> ProviderRelationshipsApiClient { get; }
        public List<AccountProviderLegalEntityDto> SeedAccountProviderLegalEntitiesDto { get; }

        public GetProviderCommitmentAgreementsHandlerTestFixtures()
        {
            _autoFixture = new Fixture();

            TransferSender = new Account(1, "", "", "TransferSender", DateTime.UtcNow);
            Account = new Account(1, "", "", "TEST", DateTime.UtcNow);
            Provider = new Provider(1, "TEST PROVIDER", DateTime.UtcNow, DateTime.UtcNow);

            SeedCohorts = new List<Cohort>();
            SeedAccountLegalEntities = new List<AccountLegalEntity>
            {
                new AccountLegalEntity(Account, 1, 1, "1", "A001", "A001", OrganisationType.Charities, "", DateTime.UtcNow),
                new AccountLegalEntity(Account, 2, 2, "2", "B002", "B002", OrganisationType.Charities, "", DateTime.UtcNow),
                new AccountLegalEntity(Account, 3, 3, "3", "C003", "C003", OrganisationType.Charities, "", DateTime.UtcNow),
                new AccountLegalEntity(Account, 4, 4, "4", "D004", "D004", OrganisationType.Charities, "", DateTime.UtcNow)
            };
            SeedAccountProviderLegalEntitiesDto = _autoFixture.CreateMany<AccountProviderLegalEntityDto>(3).ToList();

            ProviderRelationshipsApiClient = new Mock<IProviderRelationshipsApiClient>();
            ProviderRelationshipsApiClient
                .Setup(x => x.GetAccountProviderLegalEntitiesWithPermission(It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetAccountProviderLegalEntitiesWithPermissionResponse());
        }

        public Task<GetProviderCommitmentAgreementResult> GetResponse(GetProviderCommitmentAgreementQuery query)
        {
            return RunWithDbContext(dbContext =>
            {
                var lazy = new Lazy<ProviderCommitmentsDbContext>(dbContext);
                var handler = new GetProviderCommitmentAgreementsHandler(lazy, Mock.Of<ILogger<GetProviderCommitmentAgreementsHandler>>(), ProviderRelationshipsApiClient.Object);
                return handler.Handle(query, CancellationToken.None);
            });
        }

        public GetProviderCommitmentAgreementsHandlerTestFixtures AddUnapprovedCohortForEmployerWithMessagesAnd2Apprentices()
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, Account.Id)
                .With(o => o.EditStatus, EditStatus.Neither)
                .With(o => o.IsDeleted, false)
                .With(o => o.AccountLegalEntity, SeedAccountLegalEntities[0])
                .With(o => o.Provider, Provider)
                .Without(o => o.Apprenticeships)
                .Without(o => o.TransferRequests)
                .Without(o => o.TransferSender)
                .Without(o => o.Messages)
                .Create();

            cohort.Apprenticeships.Add(new DraftApprenticeship { Id = _autoFixture.Create<long>() });
            cohort.Apprenticeships.Add(new DraftApprenticeship { Id = _autoFixture.Create<long>() });

            cohort.Messages.Add(new Message(cohort, Party.Employer, "XXX", "NotLast"));
            cohort.Messages.Add(new Message(cohort, Party.Provider, "XXX", "NotLast"));

            cohort.Messages.Add(new Message(cohort, Party.Employer, "XXX", "EmployerLast"));
            cohort.Messages.Add(new Message(cohort, Party.Provider, "XXX", "ProviderLast"));

            SeedCohorts.Add(cohort);
            return this;
        }

        public GetProviderCommitmentAgreementsHandlerTestFixtures AddCohortForEmployerApprovedByBoth()
        {
            var cohort = _autoFixture.Build<Cohort>()
                .With(o => o.EmployerAccountId, Account.Id)
                .With(o => o.EditStatus, EditStatus.Both)
                .With(o => o.IsDeleted, false)
                .With(o => o.AccountLegalEntity, SeedAccountLegalEntities[1])
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

        public GetProviderCommitmentAgreementsHandlerTestFixtures AddEmployersWithPermissions()
        {
            ProviderRelationshipsApiClient
                .Setup(x => x.GetAccountProviderLegalEntitiesWithPermission(It.IsAny<GetAccountProviderLegalEntitiesWithPermissionRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new GetAccountProviderLegalEntitiesWithPermissionResponse
                {
                    AccountProviderLegalEntities = SeedAccountProviderLegalEntitiesDto
                });

            return this;
        }


        private Task<T> RunWithDbContext<T>(Func<ProviderCommitmentsDbContext, Task<T>> action)
        {
            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options;

            using var dbContext = new ProviderCommitmentsDbContext(options);
            dbContext.Database.EnsureCreated();
            SeedData(dbContext);
            return action(dbContext);
        }

        private void SeedData(ProviderCommitmentsDbContext dbContext)
        {
            dbContext.Cohorts.AddRange(SeedCohorts);

            if (SeedAccountLegalEntities.Count > 0)
            {
                dbContext.AccountLegalEntities.AddRange(SeedAccountLegalEntities);
            }

            dbContext.SaveChanges(true);
        }
    }
}
