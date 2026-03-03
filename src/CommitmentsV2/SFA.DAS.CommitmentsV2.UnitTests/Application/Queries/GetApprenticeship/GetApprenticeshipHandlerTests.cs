using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeship;

[TestFixture]
public class GetApprenticeshipHandlerTests
{
    private GetApprenticeshipHandlerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new GetApprenticeshipHandlerTestsFixture();
    }
        
    [TearDown]
    public void TearDown()
    {
        _fixture?.Dispose();
    }

    [Test]
    public async Task Handle_ThenShouldReturnResult()
    {
        await _fixture.Handle();
        _fixture.VerifyResultMapping();
    }

    [Test]
    public async Task Handle_WhenEmployerVerificationRequestExists_ThenMapsStatusAndNotes()
    {
        _fixture.WithEmployerVerificationRequest(EmployerVerificationRequestStatus.Passed, null);
        await _fixture.Handle();

        _fixture.Result.EmployerVerificationStatus.Should().Be(EmployerVerificationRequestStatus.Passed);
        _fixture.Result.EmployerVerificationNotes.Should().BeNull();
    }

    [Test]
    public async Task Handle_WhenEmployerVerificationRequestExistsWithNotes_ThenMapsStatusAndNotes()
    {
        _fixture.WithEmployerVerificationRequest(EmployerVerificationRequestStatus.Error, "PAYENotFound");
        await _fixture.Handle();

        _fixture.Result.EmployerVerificationStatus.Should().Be(EmployerVerificationRequestStatus.Error);
        _fixture.Result.EmployerVerificationNotes.Should().Be("PAYENotFound");
    }

    private class GetApprenticeshipHandlerTestsFixture : IDisposable
    {
        private Fixture _autoFixture;
        public long ApprenticeshipId { get; private set; }
        public long AccountLegalEntityId { get; private set; }
        public Apprenticeship Apprenticeship { get; private set; }
        public Cohort Cohort { get; private set; }
        public Provider Provider { get; private set; }
        public AccountLegalEntity AccountLegalEntity { get; private set; }
        public AccountLegalEntity PreviousAccountLegalEntity { get; private set; }
        public AssessmentOrganisation EndpointAssessmentOrganisation { get; private set; }
        public Apprenticeship PreviousApprenticeship { get; private set; }
        private readonly ProviderCommitmentsDbContext _db;
        private readonly GetApprenticeshipQueryHandler _handler;
        private readonly GetApprenticeshipQuery _query;
        private GetApprenticeshipQueryResult _result;

        public GetApprenticeshipQueryResult Result => _result;

        public GetApprenticeshipHandlerTestsFixture()
        {
            _autoFixture = new Fixture();

            AccountLegalEntityId = _autoFixture.Create<long>();

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            SeedData();

            _query = new GetApprenticeshipQuery(ApprenticeshipId);

            _handler = new GetApprenticeshipQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
        }

        private void SeedData()
        {

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

            Apprenticeship = new Apprenticeship
            {
                Id = ApprenticeshipId,
                CommitmentId = Cohort.Id,
                Cohort = Cohort,
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
                Uln = _autoFixture.Create<string>(),
                PaymentStatus = _autoFixture.Create<PaymentStatus>(),
                EpaOrg = EndpointAssessmentOrganisation,
                EmployerRef = _autoFixture.Create<string>(),
                ContinuationOfId = PreviousApprenticeship.Id,
                PreviousApprenticeship = PreviousApprenticeship,
                OriginalStartDate = PreviousApprenticeship.StartDate,
                Continuation = nextApprenticeship,
                MadeRedundant = _autoFixture.Create<bool?>(),
                FlexibleEmployment = _autoFixture.Create<FlexibleEmployment>(),
                PriorLearning = _autoFixture.Create<ApprenticeshipPriorLearning>(),
                TrainingTotalHours = _autoFixture.Create<int>(),
                EmployerHasEditedCost = _autoFixture.Create<bool?>()
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
            _db.SaveChanges();
        }

        public async Task Handle()
        {
            _result = await _handler.Handle(_query, new CancellationToken());
        }

        public void VerifyResultMapping()
        {
            _result.Id.Should().Be(Apprenticeship.Id);
            _result.CohortId.Should().Be(Apprenticeship.CommitmentId);
            _result.FirstName.Should().Be(Apprenticeship.FirstName);
            _result.LastName.Should().Be(Apprenticeship.LastName);
            _result.Uln.Should().Be(Apprenticeship.Uln);
            _result.StartDate.Should().Be(Apprenticeship.StartDate);
            _result.ActualStartDate.Should().Be(Apprenticeship.ActualStartDate);
            _result.EndDate.Should().Be(Apprenticeship.EndDate);
            _result.CourseName.Should().Be(Apprenticeship.CourseName);
            _result.EndpointAssessorName.Should().Be(Apprenticeship.EpaOrg.Name);
            _result.Status.Should().Be(Apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow));
            _result.StopDate.Should().Be(Apprenticeship.StopDate);
            _result.PauseDate.Should().Be(Apprenticeship.PauseDate);
            _result.CompletionDate.Should().Be(Apprenticeship.CompletionDate);
            _result.HasHadDataLockSuccess.Should().Be(Apprenticeship.HasHadDataLockSuccess);
            _result.CourseCode.Should().Be(Apprenticeship.CourseCode);
            _result.StandardUId.Should().Be(Apprenticeship.StandardUId);
            _result.Version.Should().Be(Apprenticeship.TrainingCourseVersion);
            _result.Option.Should().Be(Apprenticeship.TrainingCourseOption);
            _result.DeliveryModel.Should().Be(Apprenticeship.DeliveryModel);
            _result.AccountLegalEntityId.Should().Be(AccountLegalEntityId);
            _result.EmployerReference.Should().Be(Apprenticeship.EmployerRef);
            _result.ProviderId.Should().Be(Apprenticeship.Cohort.ProviderId);
            _result.ProviderName.Should().Be(Apprenticeship.Cohort.Provider.Name);
            _result.EmployerName.Should().Be(Apprenticeship.Cohort.AccountLegalEntity.Name);
            _result.EmployerAccountId.Should().Be(Apprenticeship.Cohort.EmployerAccountId);
            _result.ApprenticeshipEmployerTypeOnApproval.Should().Be(Apprenticeship.Cohort.ApprenticeshipEmployerTypeOnApproval);
            _result.ContinuationOfId.Should().Be(PreviousApprenticeship.Id);
            _result.PreviousProviderId.Should().Be(PreviousApprenticeship.Cohort.ProviderId);
            _result.ContinuedById.Should().Be(Apprenticeship.Continuation?.Id);
            _result.MadeRedundant.Should().Be(Apprenticeship.MadeRedundant);
            _result.FlexibleEmployment.EmploymentPrice.Should().Be(Apprenticeship.FlexibleEmployment.EmploymentPrice);
            _result.FlexibleEmployment.EmploymentEndDate.Should().Be(Apprenticeship.FlexibleEmployment.EmploymentEndDate);
            _result.RecognisePriorLearning.Should().Be(Apprenticeship.RecognisePriorLearning);
            _result.ApprenticeshipPriorLearning.DurationReducedBy.Should().Be(Apprenticeship.PriorLearning.DurationReducedBy);
            _result.ApprenticeshipPriorLearning.PriceReducedBy.Should().Be(Apprenticeship.PriorLearning.PriceReducedBy);
            _result.TransferSenderId.Should().Be(Apprenticeship.Cohort.TransferSenderId);
            _result.TrainingTotalHours.Should().Be(Apprenticeship.TrainingTotalHours);
            _result.EmployerHasEditedCost.Should().Be(Apprenticeship.EmployerHasEditedCost);
            _result.ApprenticeshipPriorLearning.IsDurationReducedByRpl.Should().Be(Apprenticeship.PriorLearning.IsDurationReducedByRpl);
            _result.EmployerVerificationStatus.Should().BeNull();
            _result.EmployerVerificationNotes.Should().BeNull();
        }

        public GetApprenticeshipHandlerTestsFixture WithEmployerVerificationRequest(EmployerVerificationRequestStatus status, string notes)
        {
            _db.EmployerVerificationRequests.Add(new EmployerVerificationRequest
            {
                ApprenticeshipId = ApprenticeshipId,
                Status = status,
                Notes = notes,
                Created = DateTime.UtcNow
            });
            _db.SaveChanges();
            return this;
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}