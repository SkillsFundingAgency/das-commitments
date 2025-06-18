using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprovedApprenticeships;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetSupportApprenticeships;

[TestFixture]
public class GetSupportApprovedApprenticeshipsQueryHandlerTests
{
    private GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture();
    }

    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task WhenApprenticeshipIsSearchedUsingCohortId_ThenShouldReturnResult()
    {
        await _fixture.Handle();
        _fixture.VerifyResultMapping();
    }

    [Test]
    public async Task WhenApprenticeshipIsSearchedUsingApprenticeship1Uln_ThenShouldReturnResult()
    {
        _fixture.WithApprenticeship1Uln();
        await _fixture.Handle();
        _fixture.VerifyApprenticeship1UlnQueryResultMapping();
    }

    [Test]
    public async Task WhenApprenticeshipIsSearchedUsingApprenticeship1Id_ThenShouldReturnResult()
    {
        _fixture.WithApprenticeship1Id();
        await _fixture.Handle();
        _fixture.VerifyApprenticeship1UlnQueryResultMapping();
    }

    [Test]
    public async Task Handle_WhenApprenticeshipCohortDoesNotExist_ThenShouldNotReturnResult()
    {
        _fixture.WithNonExistentCohort();
        await _fixture.Handle();
        _fixture.VerifyNoResult();
    }

    public class GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture : IDisposable
    {
        private readonly GetSupportApprovedApprenticeshipsQueryHandler _queryHandler;
        private readonly ProviderCommitmentsDbContext _db;
        private GetSupportApprovedApprenticeshipsQuery _query;
        private GetSupportApprovedApprenticeshipsQueryResult _queryResult;
        private readonly IFixture _autoFixture;

        public long ApprenticeshipId1 { get; private set; }
        public Apprenticeship Apprenticeship1 { get; private set; }
        public string ApprenticeshipUln1 { get; set; }

        public long ApprenticeshipId2 { get; private set; }
        public Apprenticeship Apprenticeship2 { get; private set; }
        public string ApprenticeshipUln2 { get; set; }
        public long AccountLegalEntityId { get; private set; }

        public Cohort Cohort { get; private set; }
        public long _cohortId { get; set; }

        public Provider Provider { get; private set; }
        public AccountLegalEntity AccountLegalEntity { get; private set; }
        public AccountLegalEntity PreviousAccountLegalEntity { get; private set; }
        public AssessmentOrganisation EndpointAssessmentOrganisation { get; private set; }
        public Apprenticeship PreviousApprenticeship { get; private set; }

        private Mock<IMapper<Apprenticeship, SupportApprenticeshipDetails>> _mapper;

        public GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture()
        {

            _autoFixture = new Fixture();
            _cohortId = _autoFixture.Create<long>();

            _query = new GetSupportApprovedApprenticeshipsQuery(_cohortId, null, null);

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            var lazyDb = new Lazy<ProviderCommitmentsDbContext>(() => _db);

            _mapper = new Mock<IMapper<Apprenticeship, SupportApprenticeshipDetails>>();

            SeedData();
            _queryHandler = new GetSupportApprovedApprenticeshipsQueryHandler(lazyDb, _mapper.Object, Mock.Of<ILogger<GetSupportApprovedApprenticeshipsQueryHandler>>());
        }

        public GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture WithApprenticeship1Uln()
        {
            _query = new GetSupportApprovedApprenticeshipsQuery(null, ApprenticeshipUln1, null);
            return this;
        }

        public GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture WithApprenticeship1Id()
        {
            _query = new GetSupportApprovedApprenticeshipsQuery(null, null, ApprenticeshipId1);
            return this;
        }

        public GetSupportApprovedApprenticeshipsQueryHandlerTestsFixture WithNonExistentCohort()
        {
            _query = new GetSupportApprovedApprenticeshipsQuery(_cohortId + 1, null, null);
            return this;
        }

        public async Task<GetSupportApprovedApprenticeshipsQueryResult> Handle()
        {
            _queryResult = await _queryHandler.Handle(TestHelper.Clone(_query), new CancellationToken());
            return _queryResult;
        }

        private void SeedData()
        {
            var uniqueIds = _autoFixture.CreateMany<long>(2).ToList();
            ApprenticeshipId1 = uniqueIds[0];
            ApprenticeshipId2 = uniqueIds[1];

            ApprenticeshipUln1 = _autoFixture.Create<string>();
            ApprenticeshipUln2 = _autoFixture.Create<string>();

            Provider = new Provider
            {
                UkPrn = _autoFixture.Create<long>(),
                Name = _autoFixture.Create<string>()
            };

            var account = new Account(1, "", "", "", DateTime.UtcNow);

            AccountLegalEntity = new AccountLegalEntity(account,
                AccountLegalEntityId,
                0,
                "",
                publicHashedId: _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                OrganisationType.PublicBodies,
                "",
                DateTime.UtcNow);

            Cohort = new Cohort
            {
                Id = _cohortId,
                AccountLegalEntity = AccountLegalEntity,
                EmployerAccountId = _autoFixture.Create<long>(),
                ProviderId = Provider.UkPrn,
                Provider = Provider,
                ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy
            };

            EndpointAssessmentOrganisation = new AssessmentOrganisation
            {
                EpaOrgId = _autoFixture.Create<string>(),
                Id = _autoFixture.Create<int>(),
                Name = _autoFixture.Create<string>()
            };

            var previousAccount = new Account();
            PreviousAccountLegalEntity = new AccountLegalEntity(previousAccount,
                _autoFixture.Create<long>(),
                0,
                "",
                publicHashedId: _autoFixture.Create<string>(),
                _autoFixture.Create<string>(),
                OrganisationType.PublicBodies,
                "",
                DateTime.UtcNow);

            var previousCohort = new Cohort
            {
                ProviderId = Provider.UkPrn,
                Provider = Provider,
                EmployerAccountId = previousAccount.Id,
                AccountLegalEntityId = PreviousAccountLegalEntity.Id,
                AccountLegalEntity = PreviousAccountLegalEntity,
            };

            PreviousApprenticeship = new Apprenticeship
            {
                Id = _autoFixture.Create<long>(),
                Cohort = previousCohort
            };

            var nextApprenticeship = new Apprenticeship
            {
                Id = _autoFixture.Create<long>()
            };

            Apprenticeship1 = CreateApprenticeship(ApprenticeshipUln1, ApprenticeshipId1, Cohort, nextApprenticeship);
            Apprenticeship2 = CreateApprenticeship(ApprenticeshipUln2, ApprenticeshipId2, Cohort, nextApprenticeship);

            _db.Apprenticeships.Add(Apprenticeship1);
            _db.Apprenticeships.Add(Apprenticeship2);

            _db.SaveChanges();
        }

        public void VerifyResultMapping()
        {
            _queryResult.ApprovedApprenticeships.Count().Should().Be(2);
            _mapper.Verify(x => x.Map(It.Is<Apprenticeship>(o => o.Uln == Apprenticeship1.Uln)), Times.Once);
            _mapper.Verify(x => x.Map(It.Is<Apprenticeship>(o => o.Uln == Apprenticeship2.Uln)), Times.Once);
        }

        public void VerifyApprenticeship1UlnQueryResultMapping()
        {
            _queryResult.ApprovedApprenticeships.Count().Should().Be(1);
            _mapper.Verify(x => x.Map(It.Is<Apprenticeship>(o => o.Uln == Apprenticeship1.Uln)), Times.Once);
        }

        public void VerifyNoResult()
        {
            _queryResult.ApprovedApprenticeships.Should().NotBeNull();
            _queryResult.ApprovedApprenticeships.Should().BeEmpty();
        }

        private Apprenticeship CreateApprenticeship(string uln, long apprenticeshipId, Cohort cohort,
            Apprenticeship nextApprenticeship)
        {
            var apprenticeship = new Apprenticeship
            {
                Id = apprenticeshipId,
                CommitmentId = cohort.Id,
                Cohort = cohort,
                AgreedOn = _autoFixture.Create<DateTime>(),
                CourseCode = _autoFixture.Create<string>(),
                StandardUId = "ST0001_1.0",
                TrainingCourseVersion = "1.0",
                CourseName = _autoFixture.Create<string>(),
                FirstName = _autoFixture.Create<string>(),
                LastName = _autoFixture.Create<string>(),
                DateOfBirth = _autoFixture.Create<DateTime>(),
                StartDate = _autoFixture.Create<DateTime>(),
                EndDate = _autoFixture.Create<DateTime>(),
                Uln = uln,
                PaymentStatus = _autoFixture.Create<PaymentStatus>(),
                EpaOrg = EndpointAssessmentOrganisation,
                EmployerRef = _autoFixture.Create<string>(),
                ContinuationOfId = PreviousApprenticeship.Id,
                PreviousApprenticeship = PreviousApprenticeship,
                OriginalStartDate = PreviousApprenticeship.StartDate,
                Continuation = nextApprenticeship,
                MadeRedundant = _autoFixture.Create<bool?>(),
                FlexibleEmployment = _autoFixture.Create<FlexibleEmployment>(),
            };

            switch (apprenticeship.PaymentStatus)
            {
                case PaymentStatus.Withdrawn:
                    apprenticeship.StopDate = _autoFixture.Create<DateTime>();
                    break;
                case PaymentStatus.Paused:
                    apprenticeship.PauseDate = _autoFixture.Create<DateTime>();
                    break;
                case PaymentStatus.Completed:
                    apprenticeship.CompletionDate = _autoFixture.Create<DateTime>();
                    break;
            }

            return apprenticeship;
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}