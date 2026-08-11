using AutoFixture.Kernel;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipApproval;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipApproval;

[TestFixture]
public class GetApprenticeshipApprovalHandlerTests
{
    private GetApprenticeshipApprovalHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetApprenticeshipApprovalHandlerTestsFixture();
    }

    [Test]
    public async Task Handle_ThenShouldReturn_CoreValues()
    {
        var result = await _fixture.Handle();

        result.Should().NotBeNull();
        result.ApprenticeshipId.Should().Be(_fixture.ApprenticeshipId);
        result.ApprovalRequestId.Should().Be(_fixture.ApprovalRequestId);
        result.AccountLegalEntityId.Should().Be(_fixture.AccountLegalEntityId);
        result.AccountLegalEntityName.Should().Be(_fixture.AccountLegalEntity.Name);
        result.AccountId.Should().Be(_fixture.AccountLegalEntity.AccountId);
        result.Name.Should().Be($"{_fixture.Apprenticeship.FirstName} {_fixture.Apprenticeship.LastName}");
        result.ULN.Should().Be(_fixture.Apprenticeship.Uln);
        result.StartDate.Should().Be(_fixture.Apprenticeship.StartDate);
        result.CourseCode.Should().Be(_fixture.Apprenticeship.CourseCode);
        result.CourseName.Should().Be(_fixture.Apprenticeship.CourseName);
        result.ProviderName.Should().Be(_fixture.Provider.Name);
        result.UKPRN.Should().Be(_fixture.Provider.UkPrn);
        result.ApprovalRequestStatus.Should().Be(_fixture.ApprovalRequest.Status);
        result.Items.Should().HaveCount(_fixture.ApprovalFieldRequests.Count);
    }

    [Test]
    public async Task Handle_ForContinuationApprenticeship_ThenShouldReturn_OriginalStartDate()
    {
        _fixture.Apprenticeship.ContinuationOfId = 123;
        _fixture.Apprenticeship.OriginalStartDate = DateTime.Now.AddYears(-2); 
        var result = await _fixture.Handle();

        result.Should().NotBeNull();
        result.StartDate.Should().Be(_fixture.Apprenticeship.OriginalStartDate);
    }

    [Test]
    public async Task Handle_ThenShouldReturn_MappedItems()
    {
        var result = await _fixture.Handle();

        result.Should().NotBeNull();
        result.Items.Should().BeEquivalentTo(_fixture.ApprovalFieldRequests
            .Select(x=> new GetApprenticeshipApprovalQueryResult.ChangeItem {  FieldName = x.Field, NewValue = x.New, OldValue = x.Old, EffectiveFromDate = x.EffectiveFromDate } ));
    }

    [Test]
    public async Task WhenApprovalRequestNotFound_ThenShouldReturn_Null()
    {
        _fixture.Request = new GetApprenticeshipApprovalQuery(_fixture.ApprenticeshipId, Guid.NewGuid());
        var result = await _fixture.Handle();

        result.Should().BeNull();
    }

    [Test]
    public async Task WhenApprenticeshipIdDoesNotMatch_ThenShouldReturn_Null()
    {
        _fixture.Request = new GetApprenticeshipApprovalQuery(_fixture.ApprenticeshipId + 1, _fixture.ApprovalRequestId);
        var result = await _fixture.Handle();

        result.Should().BeNull();
    }

    public class GetApprenticeshipApprovalHandlerTestsFixture
    {
        public long ApprenticeshipId { get; private set; }
        public Guid ApprovalRequestId { get; private set; } = Guid.NewGuid();
        public long AccountLegalEntityId { get; private set; }
        public ApprovalRequest ApprovalRequest { get; private set; }
        public List<ApprovalFieldRequest> ApprovalFieldRequests { get; private set; }
        public Apprenticeship Apprenticeship { get; private set; }
        public Cohort Cohort { get; private set; }
        public Provider Provider { get; private set; }
        public AccountLegalEntity AccountLegalEntity { get; private set; }
        public Course Course { get; private set; }

        public GetApprenticeshipApprovalQuery Request;
        public GetApprenticeshipApprovalQueryResult Result;

        private readonly GetApprenticeshipApprovalQueryHandler _handler;
        private readonly ProviderCommitmentsDbContext _db;
        private Fixture _autoFixture;

        public GetApprenticeshipApprovalHandlerTestsFixture()
        {
            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            SeedData();
            Request = new GetApprenticeshipApprovalQuery(ApprenticeshipId, ApprovalRequestId);


            _handler = new GetApprenticeshipApprovalQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
        }

        public async Task<GetApprenticeshipApprovalQueryResult> Handle()
        {
            Result = await _handler.Handle(Request, new CancellationToken());
            return Result;
        }

        private GetApprenticeshipApprovalHandlerTestsFixture SeedData()
        {
            _autoFixture = new Fixture();
            _autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _autoFixture.Customizations.Add(
                new TypeRelay(
                    typeof(SFA.DAS.CommitmentsV2.Models.ApprenticeshipBase),
                    typeof(Apprenticeship)));

            ApprenticeshipId = _autoFixture.Create<long>();

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
                Id = _autoFixture.CreateMany<long>().Last(),
                AccountLegalEntity = AccountLegalEntity,
                EmployerAccountId = _autoFixture.Create<long>(),
                ProviderId = Provider.UkPrn,
                Provider = Provider,
                ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy
            };

            var courseCode = _autoFixture.Create<string>();

            Apprenticeship = new Apprenticeship
            {
                Id = ApprenticeshipId,
                CommitmentId = Cohort.Id,
                Cohort = Cohort,
                AgreedOn = _autoFixture.Create<DateTime>(),
                CourseCode = courseCode,
                StandardUId = "ST0001_1.0",
                TrainingCourseVersion = "1.0",
                CourseName = _autoFixture.Create<string>(),
                FirstName = _autoFixture.Create<string>(),
                LastName = _autoFixture.Create<string>(),
                DateOfBirth = _autoFixture.Create<DateTime>(),
                StartDate = _autoFixture.Create<DateTime>(),
                EndDate = _autoFixture.Create<DateTime>(),
                Uln = _autoFixture.Create<string>(),
                PaymentStatus = _autoFixture.Create<PaymentStatus>(),
                EmployerRef = _autoFixture.Create<string>(),
                MadeRedundant = _autoFixture.Create<bool?>(),
                FlexibleEmployment = _autoFixture.Create<FlexibleEmployment>(),
                PriorLearning = _autoFixture.Create<ApprenticeshipPriorLearning>(),
                TrainingTotalHours = _autoFixture.Create<int>(),
                EmployerHasEditedCost = _autoFixture.Create<bool?>(),
                StopDate = _autoFixture.Create<DateTime>(),
                WithdrawnReasonCode = _autoFixture.Create<int?>(),
                PaymentFreezeDate = DateTime.UtcNow.Date.AddDays(-7),
                FreezePaymentsReason = FreezePaymentsReason.LearnerOnBreak,
            };

            switch (Apprenticeship.PaymentStatus)
            {
                case PaymentStatus.Withdrawn:
                    Apprenticeship.StopDate = _autoFixture.Create<DateTime>();
                    break;
                case PaymentStatus.Paused:
                    Apprenticeship.PauseDate = _autoFixture.Create<DateTime>();
                    break;
                case PaymentStatus.Completed:
                    Apprenticeship.CompletionDate = _autoFixture.Create<DateTime>();
                    break;
            }

            _db.Apprenticeships.Add(Apprenticeship);

            Course = _autoFixture.Build<Course>().With(c => c.LarsCode, courseCode).Create();
            _db.Courses.Add(Course);

            ApprovalFieldRequests = _autoFixture.Build<ApprovalFieldRequest>()
                .With(afr => afr.ApprovalRequestId, ApprovalRequestId)
                .CreateMany(3).ToList();

            ApprovalRequest = _autoFixture.Build<ApprovalRequest>()
                .With(ar => ar.Id, ApprovalRequestId)
                .With(ar => ar.ApprenticeshipId, ApprenticeshipId)
                .With(ar => ar.Status, CocApprovalResultStatus.Pending)
                .With(ar => ar.Items, ApprovalFieldRequests)
                .Without(ar=>ar.Apprenticeship)
                .Create();
            _db.ApprovalRequests.Add(ApprovalRequest);

            _db.SaveChanges();

            return this;
        }
    }
}
