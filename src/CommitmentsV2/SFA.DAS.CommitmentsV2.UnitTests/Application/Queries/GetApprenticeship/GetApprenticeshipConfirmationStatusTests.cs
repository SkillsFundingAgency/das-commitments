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
        private Apprenticeship _appr;
        private GetApprenticeshipQueryHandler _sut;
        private GetApprenticeshipQuery _query;

        public GetApprenticeshipConfirmationStatusTests()
        {
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var cort = new Cohort()
              .Set(x => x.Id, 123)
              .Set(x => x.EmployerAccountId, 456)
              .Set(x => x.ProviderId, 789)
              .Set(x => x.AccountLegalEntity, new AccountLegalEntity())
              .Set(x => x.Provider, new Provider());

            _appr = _fixture.Build<Apprenticeship>()
                .With(x => x.Id, _apprenticeshipID)
                .With(x => x.Cohort, cort)
                .With(x => x.EpaOrg, new AssessmentOrganisation() { EpaOrgId = "991" })
                .With(x => x.EpaOrgId, "991")
                .Without(x => x.Continuation)
                .Without(x => x.ContinuationOfId)
                .Without(x => x.PaymentStatus)
                .Without(x => x.ApprenticeshipUpdate)
                .Without(x => x.PriceHistory)
                .Without(x => x.DataLockStatus)
                .Without(x => x.PreviousApprenticeship)
                .Create();
        }

        [Test]
        public async Task WhenConfirmedOnIsNull_ThenConfirmationStatusShouldBeUnconfirmed()
        {
            // Arrange
            Setup(null);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.ConfirmationStatus, ConfirmationStatus.Unconfirmed);
        }

        [Test]
        public async Task WhenConfirmedOnIsNotNull_ThenConfirmationStatusShouldBeConfirmed()
        {
            // Arrange
            Setup(DateTime.Now);

            // Act
            var response = await _sut.Handle(_query, new CancellationToken());

            //Assert
            Assert.AreEqual(response.ConfirmationStatus, ConfirmationStatus.Confirmed);
        }

        private void Setup(DateTime? confirmedOnDate)
        {
            var conf = 
                confirmedOnDate == null ?
                    _fixture.Build<ApprenticeshipConfirmationStatus>()
                            .With(x => x.ApprenticeshipId, _apprenticeshipID)
                            .With(x => x.Apprenticeship, _appr)
                            .Without(x => x.ApprenticeshipConfirmedOn)
                            .Create() :
                    _fixture.Build<ApprenticeshipConfirmationStatus>()
                            .With(x => x.ApprenticeshipId, _apprenticeshipID)
                            .With(x => x.Apprenticeship, _appr)
                            .With(x => x.ApprenticeshipConfirmedOn, confirmedOnDate)
                            .Create();            

            var _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
            _db.Apprenticeships.Add(_appr);
            _db.ApprenticeshipConfirmationStatus.Add(conf);
            _db.SaveChanges();

            _sut = new GetApprenticeshipQueryHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), new Mock<IAuthenticationService>().Object);

            _query = new GetApprenticeshipQuery(_apprenticeshipID);
        }
    }
}
