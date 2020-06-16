using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
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

        [Test]
        public async Task Handle_ThenShouldReturnResult()
        {
            await _fixture.Handle();
            _fixture.VerifyResultMapping();
        }

        private class GetApprenticeshipHandlerTestsFixture
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
            private readonly Mock<IAuthenticationService> AuthenticationService;

            public GetApprenticeshipHandlerTestsFixture()
            {
                _autoFixture = new Fixture();

                AccountLegalEntityId = _autoFixture.Create<long>();

                AuthenticationService = new Mock<IAuthenticationService>();
                AuthenticationService.Setup(x => x.GetUserParty()).Returns(Party.Employer);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
                SeedData();

                _query = new GetApprenticeshipQuery(ApprenticeshipId);

                _handler = new GetApprenticeshipQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db),
                    AuthenticationService.Object);
            }

            private void SeedData()
            {

                ApprenticeshipId = _autoFixture.Create<long>();

                Provider = new Provider
                {
                    UkPrn = _autoFixture.Create<long>(),
                    Name = _autoFixture.Create<string>()
                };

                var account = new Account(0, "", "", "", DateTime.UtcNow);

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
                    Id = _autoFixture.Create<long>(),
                    AccountLegalEntity = AccountLegalEntity,
                    EmployerAccountId = _autoFixture.Create<long>(),
                    ProviderId = Provider.UkPrn,
                    Provider = Provider
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

                Apprenticeship = new Apprenticeship
                {
                    Id = ApprenticeshipId,
                    CommitmentId = Cohort.Id,
                    Cohort = Cohort,
                    AgreedOn = _autoFixture.Create<DateTime>(),
                    CourseCode = _autoFixture.Create<string>(),
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
                    OriginalStartDate = PreviousApprenticeship.StartDate
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
                Assert.AreEqual(Apprenticeship.Id, _result.Id);
                Assert.AreEqual(Apprenticeship.CommitmentId, _result.CohortId);
                Assert.AreEqual(Apprenticeship.FirstName, _result.FirstName);
                Assert.AreEqual(Apprenticeship.LastName, _result.LastName);
                Assert.AreEqual(Apprenticeship.Uln, _result.Uln);
                Assert.AreEqual(Apprenticeship.StartDate, _result.StartDate);
                Assert.AreEqual(Apprenticeship.EndDate, _result.EndDate);
                Assert.AreEqual(Apprenticeship.CourseName, _result.CourseName);
                Assert.AreEqual(Apprenticeship.EpaOrg.Name, _result.EndpointAssessorName);
                Assert.AreEqual(Apprenticeship.GetApprenticeshipStatus(DateTime.UtcNow), _result.Status);
                Assert.AreEqual(Apprenticeship.StopDate, _result.StopDate);
                Assert.AreEqual(Apprenticeship.PauseDate, _result.PauseDate);
                Assert.AreEqual(Apprenticeship.CompletionDate, _result.CompletionDate);
                Assert.AreEqual(Apprenticeship.HasHadDataLockSuccess, _result.HasHadDataLockSuccess);
                Assert.AreEqual(Apprenticeship.CourseCode, _result.CourseCode);
                Assert.AreEqual(AccountLegalEntityId, _result.AccountLegalEntityId);
                Assert.AreEqual(Apprenticeship.EmployerRef, _result.Reference);
                Assert.AreEqual(Apprenticeship.Cohort.ProviderId, _result.ProviderId);
                Assert.AreEqual(Apprenticeship.Cohort.Provider.Name, _result.ProviderName);
                Assert.AreEqual(Apprenticeship.Cohort.AccountLegalEntity.Name, _result.EmployerName);
                Assert.AreEqual(Apprenticeship.Cohort.EmployerAccountId, _result.EmployerAccountId);
                Assert.AreEqual(PreviousApprenticeship.Id, _result.ContinuationOfId);
                Assert.AreEqual(PreviousApprenticeship.Cohort.ProviderId, _result.PreviousProviderId);
            }
        }
    }
}
