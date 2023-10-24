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
                    FirstName = _autoFixture.Create<string>(),
                    LastName = _query.LastName,
                    DateOfBirth = _query.DateOfBirth,
                    Email = _query.Email,
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

            public void VerifyResultsCount() => Assert.AreEqual(1, _result.Apprenticeships.Count());

            public void VerifyResultMapping()
            {
                Assert.AreEqual(_apprenticeship.Id, _result.Apprenticeships.First().ApprenticeshipId);
                Assert.AreEqual(_apprenticeship.Uln, _result.Apprenticeships.First().Uln);
                Assert.AreEqual(_apprenticeship.CourseCode, _result.Apprenticeships.First().TrainingCode);
                Assert.AreEqual(_apprenticeship.StandardUId, _result.Apprenticeships.First().StandardUId);
                Assert.AreEqual(_apprenticeship.StartDate, _result.Apprenticeships.First().StartDate);
                Assert.AreEqual(_apprenticeship.EndDate, _result.Apprenticeships.First().EndDate);
                Assert.AreEqual(_apprenticeship.StopDate, _result.Apprenticeships.First().StopDate);
                Assert.AreEqual(_apprenticeship.PaymentStatus, _result.Apprenticeships.First().PaymentStatus);
                Assert.AreEqual(_apprenticeship.Cohort.AccountLegalEntity.Name, _result.Apprenticeships.First().EmployerName);
                Assert.AreEqual(_apprenticeship.Cohort.ProviderId, _result.Apprenticeships.First().Ukprn);
            }

            public void Dispose()
            {
                _db?.Dispose();
            }
        }
    }
}