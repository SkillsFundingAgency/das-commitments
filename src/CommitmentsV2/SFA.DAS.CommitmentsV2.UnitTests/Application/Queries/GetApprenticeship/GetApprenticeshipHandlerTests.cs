using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeship
{
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
                    IsOnFlexiPaymentPilot = _autoFixture.Create<bool>(),
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
                Assert.Multiple(() =>
                {
                    Assert.That(_result.Id, Is.EqualTo(Apprenticeship.Id));
                    Assert.That(_result.CohortId, Is.EqualTo(Apprenticeship.CommitmentId));
                    Assert.That(_result.FirstName, Is.EqualTo(Apprenticeship.FirstName));
                    Assert.That(_result.LastName, Is.EqualTo(Apprenticeship.LastName));
                    Assert.That(_result.Uln, Is.EqualTo(Apprenticeship.Uln));
                    Assert.That(_result.StartDate, Is.EqualTo(Apprenticeship.StartDate));
                    Assert.That(_result.ActualStartDate, Is.EqualTo(Apprenticeship.ActualStartDate));
                    Assert.That(_result.EndDate, Is.EqualTo(Apprenticeship.EndDate));
                    Assert.That(_result.CourseName, Is.EqualTo(Apprenticeship.CourseName));
                    Assert.That(_result.EndpointAssessorName, Is.EqualTo(Apprenticeship.EpaOrg.Name));
                    Assert.That(_result.Status, Is.EqualTo(Apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow)));
                    Assert.That(_result.StopDate, Is.EqualTo(Apprenticeship.StopDate));
                    Assert.That(_result.PauseDate, Is.EqualTo(Apprenticeship.PauseDate));
                    Assert.That(_result.CompletionDate, Is.EqualTo(Apprenticeship.CompletionDate));
                    Assert.That(_result.HasHadDataLockSuccess, Is.EqualTo(Apprenticeship.HasHadDataLockSuccess));
                    Assert.That(_result.CourseCode, Is.EqualTo(Apprenticeship.CourseCode));
                    Assert.That(_result.StandardUId, Is.EqualTo(Apprenticeship.StandardUId));
                    Assert.That(_result.Version, Is.EqualTo(Apprenticeship.TrainingCourseVersion));
                    Assert.That(_result.Option, Is.EqualTo(Apprenticeship.TrainingCourseOption));
                    Assert.That(_result.DeliveryModel, Is.EqualTo(Apprenticeship.DeliveryModel));
                    Assert.That(_result.AccountLegalEntityId, Is.EqualTo(AccountLegalEntityId));
                    Assert.That(_result.EmployerReference, Is.EqualTo(Apprenticeship.EmployerRef));
                    Assert.That(_result.ProviderId, Is.EqualTo(Apprenticeship.Cohort.ProviderId));
                    Assert.That(_result.ProviderName, Is.EqualTo(Apprenticeship.Cohort.Provider.Name));
                    Assert.That(_result.EmployerName, Is.EqualTo(Apprenticeship.Cohort.AccountLegalEntity.Name));
                    Assert.That(_result.EmployerAccountId, Is.EqualTo(Apprenticeship.Cohort.EmployerAccountId));
                    Assert.That(_result.ApprenticeshipEmployerTypeOnApproval, Is.EqualTo(Apprenticeship.Cohort.ApprenticeshipEmployerTypeOnApproval));
                    Assert.That(_result.ContinuationOfId, Is.EqualTo(PreviousApprenticeship.Id));
                    Assert.That(_result.PreviousProviderId, Is.EqualTo(PreviousApprenticeship.Cohort.ProviderId));
                    Assert.That(_result.ContinuedById, Is.EqualTo(Apprenticeship.Continuation?.Id));
                    Assert.That(_result.MadeRedundant, Is.EqualTo(Apprenticeship.MadeRedundant));
                    Assert.That(_result.FlexibleEmployment.EmploymentPrice, Is.EqualTo(Apprenticeship.FlexibleEmployment.EmploymentPrice));
                    Assert.That(_result.FlexibleEmployment.EmploymentEndDate, Is.EqualTo(Apprenticeship.FlexibleEmployment.EmploymentEndDate));
                    Assert.That(_result.RecognisePriorLearning, Is.EqualTo(Apprenticeship.RecognisePriorLearning));
                    Assert.That(_result.ApprenticeshipPriorLearning.DurationReducedBy, Is.EqualTo(Apprenticeship.PriorLearning.DurationReducedBy));
                    Assert.That(_result.ApprenticeshipPriorLearning.PriceReducedBy, Is.EqualTo(Apprenticeship.PriorLearning.PriceReducedBy));
                    Assert.That(_result.TransferSenderId, Is.EqualTo(Apprenticeship.Cohort.TransferSenderId));
                    Assert.That(_result.IsOnFlexiPaymentPilot, Is.EqualTo(Apprenticeship.IsOnFlexiPaymentPilot));
                    Assert.That(_result.TrainingTotalHours, Is.EqualTo(Apprenticeship.TrainingTotalHours));
                    Assert.That(_result.EmployerHasEditedCost, Is.EqualTo(Apprenticeship.EmployerHasEditedCost));
                    Assert.That(_result.ApprenticeshipPriorLearning.IsDurationReducedByRpl, Is.EqualTo(Apprenticeship.PriorLearning.IsDurationReducedByRpl));
                });
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
