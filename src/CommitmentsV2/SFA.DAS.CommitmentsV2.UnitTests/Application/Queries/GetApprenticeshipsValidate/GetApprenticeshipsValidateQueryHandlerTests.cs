using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipsValidate;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeshipsValidate
{
    public class GetApprenticeshipsValidateQueryHandlerTests
    {
        private GetApprenticeshipsValidateHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new GetApprenticeshipsValidateHandlerTestsFixture();
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
            _fixture.VerifyResultsCount();
            _fixture.VerifyResultMapping();
        }

        private class GetApprenticeshipsValidateHandlerTestsFixture : IDisposable
        {
            private readonly Fixture _autoFixture;
            private readonly ProviderCommitmentsDbContext _db;
            private readonly GetApprenticeshipsValidateQueryHandler _handler;
            private readonly GetApprenticeshipsValidateQuery _query;
            private GetApprenticeshipsValidateQueryResult _result;
            private Apprenticeship _apprenticeship;

            public GetApprenticeshipsValidateHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                _query = _autoFixture.Create<GetApprenticeshipsValidateQuery>();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();

                _handler = new GetApprenticeshipsValidateQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db));
            }

            private void SeedData()
            {
                var ApprenticeshipId = _autoFixture.Create<long>();

                var provider = new Provider
                {
                    UkPrn = _autoFixture.Create<long>(),
                    Name = _autoFixture.Create<string>()
                };

                var account = new Account(1, "", "", "", DateTime.UtcNow);

                var accountLegalEntity = new AccountLegalEntity(account,
                    _autoFixture.Create<long>(),
                    0,
                    "",
                    publicHashedId: _autoFixture.Create<string>(),
                    _autoFixture.Create<string>(),
                    OrganisationType.PublicBodies,
                    "",
                    DateTime.UtcNow);

                var cohort = new Cohort
                {
                    Id = _autoFixture.CreateMany<long>().Last(),
                    AccountLegalEntity = accountLegalEntity,
                    EmployerAccountId = _autoFixture.Create<long>(),
                    ProviderId = provider.UkPrn,
                    Provider = provider,
                    ApprenticeshipEmployerTypeOnApproval = ApprenticeshipEmployerType.Levy
                };

                var endpointAssessmentOrganisation = new AssessmentOrganisation
                {
                    EpaOrgId = _autoFixture.Create<string>(),
                    Id = _autoFixture.Create<int>(),
                    Name = _autoFixture.Create<string>()
                };

                var previousAccount = new Account();
                var previousAccountLegalEntity = new AccountLegalEntity(previousAccount,
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
                    ProviderId = provider.UkPrn,
                    Provider = provider,
                    EmployerAccountId = previousAccount.Id,
                    AccountLegalEntityId = previousAccountLegalEntity.Id,
                    AccountLegalEntity = previousAccountLegalEntity,
                };

                var previousApprenticeship = new Apprenticeship
                {
                    Id = _autoFixture.Create<long>(),
                    Cohort = previousCohort
                };

                var nextApprenticeship = new Apprenticeship
                {
                    Id = _autoFixture.Create<long>()
                };

                _apprenticeship = new Apprenticeship
                {
                    Id = ApprenticeshipId,
                    CommitmentId = cohort.Id,
                    Cohort = cohort,
                    AgreedOn = _autoFixture.Create<DateTime>(),
                    CourseCode = _autoFixture.Create<string>(),
                    StandardUId = "ST0001_1.0",
                    TrainingCourseVersion = "1.0",
                    CourseName = _autoFixture.Create<string>(),
                    FirstName = _query.FirstName,
                    LastName = _query.LastName,
                    DateOfBirth = _query.DateOfBirth,
                    Email = _autoFixture.Create<string>(),
                    StartDate = _autoFixture.Create<DateTime>(),
                    EndDate = _autoFixture.Create<DateTime>(),
                    Uln = _autoFixture.Create<string>(),
                    PaymentStatus = _autoFixture.Create<PaymentStatus>(),
                    EpaOrg = endpointAssessmentOrganisation,
                    EmployerRef = _autoFixture.Create<string>(),
                    ContinuationOfId = previousApprenticeship.Id,
                    PreviousApprenticeship = previousApprenticeship,
                    OriginalStartDate = previousApprenticeship.StartDate,
                    Continuation = nextApprenticeship,
                    MadeRedundant = _autoFixture.Create<bool?>(),
                    FlexibleEmployment = _autoFixture.Create<FlexibleEmployment>(),
                    PriorLearning = _autoFixture.Create<ApprenticeshipPriorLearning>(),
                    IsOnFlexiPaymentPilot = _autoFixture.Create<bool>(),
                    TrainingTotalHours = _autoFixture.Create<int>(),
                };

                switch (_apprenticeship.PaymentStatus)
                {
                    case PaymentStatus.Withdrawn:
                        _apprenticeship.StopDate = _autoFixture.Create<DateTime>();
                        break;
                    case PaymentStatus.Paused:
                        _apprenticeship.PauseDate = _autoFixture.Create<DateTime>();
                        break;
                    case PaymentStatus.Completed:
                        _apprenticeship.CompletionDate = _autoFixture.Create<DateTime>();
                        break;
                }

                _db.Apprenticeships.Add(this._apprenticeship);
                _db.SaveChanges();
            }

            public async Task Handle()
            {
                _result = await _handler.Handle(_query, new CancellationToken());
            }

            public void VerifyResultsCount() => Assert.That(_result.Apprenticeships.Count(), Is.EqualTo(1));

            public void VerifyResultMapping()
            {
                Assert.That(_result.Apprenticeships.First().ApprenticeshipId, Is.EqualTo(_apprenticeship.Id));
                Assert.That(_result.Apprenticeships.First().Uln, Is.EqualTo(_apprenticeship.Uln));
                Assert.That(_result.Apprenticeships.First().TrainingCode, Is.EqualTo(_apprenticeship.CourseCode));
                Assert.That(_result.Apprenticeships.First().StandardUId, Is.EqualTo(_apprenticeship.StandardUId));
                Assert.That(_result.Apprenticeships.First().StartDate, Is.EqualTo(_apprenticeship.StartDate));
                Assert.That(_result.Apprenticeships.First().EndDate, Is.EqualTo(_apprenticeship.EndDate));
                Assert.That(_result.Apprenticeships.First().StopDate, Is.EqualTo(_apprenticeship.StopDate));
                Assert.That(_result.Apprenticeships.First().PaymentStatus, Is.EqualTo(_apprenticeship.PaymentStatus));
                Assert.That(_result.Apprenticeships.First().EmployerName, Is.EqualTo(_apprenticeship.Cohort.AccountLegalEntity.Name));
                Assert.That(_result.Apprenticeships.First().Ukprn, Is.EqualTo(_apprenticeship.Cohort.ProviderId));
            }

            public void Dispose()
            {
                _db?.Dispose();
            }
        }
    }
}