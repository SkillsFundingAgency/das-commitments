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
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeship
{
    [TestFixture]
    public class GetApprenticeshipConfirmationStatusTests
    {
        private Fixture _fixture = new Fixture();
        private long _apprenticeshipID = 12345;
        private GetApprenticeshipQueryHandler _sut;
        private GetApprenticeshipQuery _query;

        public GetApprenticeshipConfirmationStatusTests()
        {
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            _query = new GetApprenticeshipQuery(_apprenticeshipID);
        }

        [Test]
        public async Task WhenEmailIsNull_ThenEmailAddressConfirmedByApprenticeShouldBeFalse()
        {
            // Arrange
            Setup(null, null, null);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.EmailAddressConfirmedByApprentice, false);
        }

        [Test]
        public async Task WhenEmailIsNotNullAndItsConfirmedByApprentice_ThenEmailAddressConfirmedByApprenticeShouldBeTrue()
        {
            // Arrange
            Setup("test@test.com", null, null, emailAddressConfirmed: true);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.EmailAddressConfirmedByApprentice, true);
        }

        [Test]
        public async Task WhenCohortIsApprovedAfter9Sept21_ThenEmailShouldBePresentShouldBeTrue()
        {
            // Arrange
            Setup(null, null, null);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.EmailShouldBePresent, true);
        }

        [Test]
        public async Task WhenCohortIsApprovedAfter9Sept21ButIsAContinuationApprenticeship_ThenEmailShouldBePresentShouldBeTrue()
        {
            // Arrange
            Setup(null, null, null, null, 10890);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.EmailShouldBePresent, false);
        }

        [Test]
        public async Task WhenCohortIsApprovedBefore9Sept21_ThenEmailShouldBePresentShouldBeFalse()
        {
            // Arrange
            Setup(null, null, null, new DateTime(2021, 09, 08));

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.EmailShouldBePresent, false);
        }

        [Test]
        public async Task WhenEmailIsNull_ThenConfirmationStatusShouldBeNull()
        {
            // Arrange
            Setup(null, null, null);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.ConfirmationStatus, null);
        }

        [Test]
        public async Task WhenConfirmedOnIsNull_ThenConfirmationStatusShouldBeUnconfirmed()
        {
            // Arrange
            Setup("a@a.com", null, null);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.ConfirmationStatus, ConfirmationStatus.Unconfirmed);
        }

        [Test]
        public async Task WhenConfirmedOnIsNotNull_ThenConfirmationStatusShouldBeConfirmed()
        {
            // Arrange
            Setup("a@a.com", DateTime.Now, null);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.ConfirmationStatus, ConfirmationStatus.Confirmed);
        }

        [Test]
        public async Task WhenOverdueIsNotNull_ThenConfirmationStatusShouldBeOverdue()
        {
            // Arrange
            Setup("a@a.com", null, DateTime.Now.AddDays(-1));

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.ConfirmationStatus, ConfirmationStatus.Overdue);
        }

        private void Setup(string email, DateTime? confirmedOnDate, DateTime? overdueDate, DateTime? newApprovedOnDate = null, long? continuationOfId = null, bool? emailAddressConfirmed = null)
        {
            var approvedOnDate = newApprovedOnDate ?? new DateTime(2021, 9, 10);

            var cort = new Cohort()
              .Set(x => x.Id, 123)
              .Set(x => x.EmployerAccountId, 456)
              .Set(x => x.ProviderId, 789)
              .Set(x => x.AccountLegalEntity, new AccountLegalEntity())
              .Set(x => x.EmployerAndProviderApprovedOn, approvedOnDate)
              .Set(x => x.Provider, new Provider());

            var epao = new AssessmentOrganisation { EpaOrgId = "991" };
            var continuationApp = _fixture.Build<Apprenticeship>()
                .With(x => x.Id, continuationOfId)
                .With(x => x.Cohort, cort)
                .With(x => x.EpaOrg, epao)
                .With(x => x.EpaOrgId, "991")
                .With(x => x.Email, email)
                .Without(x => x.Continuation)
                .Without(x => x.ContinuationOfId)
                .Without(x => x.PaymentStatus)
                .Without(x => x.ApprenticeshipUpdate)
                .Without(x => x.PriceHistory)
                .Without(x => x.DataLockStatus)
                .Without(x => x.PreviousApprenticeship)
                .Without(x => x.ApprenticeshipConfirmationStatus)
                .Without(s => s.FlexibleEmployment)
                .Create();

            var appr = _fixture.Build<Apprenticeship>()
                .With(x => x.Id, _apprenticeshipID)
                .With(x => x.Cohort, cort)
                .With(x => x.CommitmentId, cort.Id)
                .With(x => x.EpaOrg, epao)
                .With(x => x.EpaOrgId, "991")
                .With(x => x.Email, email)
                .With(x => x.EmailAddressConfirmed, emailAddressConfirmed)
                .With(x => x.ContinuationOfId, continuationOfId)
                .With(x => x.Continuation, continuationOfId == null ? null : continuationApp)
                .Without(x => x.PaymentStatus)
                .Without(x => x.ApprenticeshipUpdate)
                .Without(x => x.PriceHistory)
                .Without(x => x.DataLockStatus)
                .Without(x => x.PreviousApprenticeship)
                .Without(x => x.ApprenticeshipConfirmationStatus)
                .Without(s => s.FlexibleEmployment)
                .Create();

            var conf = _fixture.Build<ApprenticeshipConfirmationStatus>()
                .With(x => x.Apprenticeship, appr)
                .With(x => x.ApprenticeshipConfirmedOn, confirmedOnDate)
                .With(x => x.ConfirmationOverdueOn, overdueDate)
                .Create();

            var options = new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking).Options;
            var _db = new ProviderCommitmentsDbContext(options);

            _db.Apprenticeships.Add(appr);
            _db.Apprenticeships.Add(continuationApp);
            _db.Cohorts.Add(cort);
            if (email != null)
            {
                _db.ApprenticeshipConfirmationStatus.Add(conf);
            }

            _db.SaveChanges();

            _sut = new GetApprenticeshipQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), new Mock<IAuthenticationService>().Object);
        }
    }
}