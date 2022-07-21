using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Services
{
    [TestFixture]
    public class ResolveOverlappingTrainingDateRequestServiceTests
    {
        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipIsStopped()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture();
            await fixture.ResolveApprenticeshipByStoppingApprenticeship();
            Assert.AreEqual(OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped, fixture.OverlappingTrainingDateRequest.ResolutionType);
            Assert.AreEqual(OverlappingTrainingDateRequestStatus.Resolved, fixture.OverlappingTrainingDateRequest.Status);
        }

        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipIsStopped_Resolve_OverlappingRequest_Even_When_There_Is_Still_A_Overlap()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture().SetupOverlapCheckService(true, 1);
            await fixture.ResolveApprenticeshipByStoppingApprenticeship();
            fixture.VerifyOverlappingServiceIsNotCalled();
            Assert.AreEqual(OverlappingTrainingDateRequestStatus.Resolved, fixture.OverlappingTrainingDateRequest.Status);
        }

        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipIsStopped_Doesnt_call_OverlapService()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture();
            await fixture.ResolveApprenticeshipByStoppingApprenticeship();
            fixture.VerifyOverlappingServiceIsNotCalled();
          
        }

        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipStopDateIsUpdated_Doesnt_call_OverlapService()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture();
            await fixture.ResolveApprenticeshipByUpdatingStopDate();
            fixture.VerifyOverlappingServiceIsNotCalled();
        }

        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipStopDateIsUpdated_Resolve_OverlappingRequest_When_There_Is_Still_A_Overlap()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture().SetupOverlapCheckService(true, 1);
            await fixture.ResolveApprenticeshipByUpdatingStopDate();
            fixture.VerifyOverlappingServiceIsNotCalled();
            Assert.AreEqual(OverlappingTrainingDateRequestStatus.Resolved, fixture.OverlappingTrainingDateRequest.Status);
        }

        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipStopDateIsUpdated()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture();
            await fixture.ResolveApprenticeshipByUpdatingStopDate();
            Assert.AreEqual(OverlappingTrainingDateRequestResolutionType.StopDateUpdate, fixture.OverlappingTrainingDateRequest.ResolutionType);
            Assert.AreEqual(OverlappingTrainingDateRequestStatus.Resolved, fixture.OverlappingTrainingDateRequest.Status);
        }


        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipUpdateIsApproved_calls_OverlapService()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture();
            await fixture.ResolveApprenticeshipByApprenticeshipUpdate();
            fixture.VerifyOverlappingServiceIsCalled();
        }

        [Test]
        public async Task OverlappingTrainingDateIsNotResolved_WhenApprenticeshipUpdateIsApproved_When_There_Is_Still_A_Overlap()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture().SetupOverlapCheckService(true, 1);
            await fixture.ResolveApprenticeshipByApprenticeshipUpdate();
            Assert.AreEqual(OverlappingTrainingDateRequestStatus.Pending, fixture.OverlappingTrainingDateRequest.Status);
        }

        [Test]
        public async Task OverlappingTrainingDateIsResolved_WhenApprenticeshipUpdateIsApproved()
        {
            var fixture = new ResolveOverlappingTrainingDateRequestServiceTestsFixture();
            await fixture.ResolveApprenticeshipByApprenticeshipUpdate();
            Assert.AreEqual(OverlappingTrainingDateRequestResolutionType.ApprenticeshipUpdate, fixture.OverlappingTrainingDateRequest.ResolutionType);
            Assert.AreEqual(OverlappingTrainingDateRequestStatus.Resolved, fixture.OverlappingTrainingDateRequest.Status);
        }

        private class ResolveOverlappingTrainingDateRequestServiceTestsFixture
        {
            private OverlapCheckResultOnStartDate _overlapCheckResultOnStartDate;
            private Fixture _fixture;
            public Apprenticeship ApprenticeshipDetails { get; set; }
            public OverlappingTrainingDateRequest OverlappingTrainingDateRequest { get; set; }
            private readonly ResolveOverlappingTrainingDateRequestService _sut;
            private readonly Mock<IOverlapCheckService> _overlapCheckService;
            public ProviderCommitmentsDbContext Db { get; set; }

            public ResolveOverlappingTrainingDateRequestServiceTestsFixture()
            {
                _fixture = new Fixture();
                _overlapCheckService = new Mock<IOverlapCheckService>();
                _overlapCheckResultOnStartDate = new OverlapCheckResultOnStartDate(false, null);

                _overlapCheckService.Setup(x => x.CheckForOverlapsOnStartDate(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, It.IsAny<CancellationToken>())).ReturnsAsync(() => _overlapCheckResultOnStartDate);

                Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                           .UseInMemoryDatabase(Guid.NewGuid().ToString())
                            .Options);

                SeedData();

                _sut = new ResolveOverlappingTrainingDateRequestService(new Lazy<ProviderCommitmentsDbContext>(() => Db),
                        _overlapCheckService.Object,
                        Mock.Of<ILogger<ResolveOverlappingTrainingDateRequestService>>());
            }

            public ResolveOverlappingTrainingDateRequestServiceTestsFixture SetupOverlapCheckService(bool hasStartDateOverlap, long? apprneticeshipId)
            {
                _overlapCheckResultOnStartDate = new OverlapCheckResultOnStartDate(hasStartDateOverlap, apprneticeshipId);
                return this;
            }

            private void SeedData()
            {
                var ale = new AccountLegalEntity()
                    .Set(x => x.Id, 1);

                var ale1 = new AccountLegalEntity()
                    .Set(x => x.Id, 2);

                var cohort = new Cohort()
                               .Set(c => c.Id, 111)
                               .Set(c => c.EmployerAccountId, 222)
                               .Set(c => c.ProviderId, 333)
                               .Set(c => c.AccountLegalEntity,ale);

                var cohort1 = new Cohort()
                             .Set(c => c.Id, 112)
                             .Set(c => c.EmployerAccountId, 222)
                             .Set(c => c.ProviderId, 334)
                             .Set(c => c.AccountLegalEntity, ale1);

                var priceHistory = new List<PriceHistory>()
                {
                   new PriceHistory
                    {
                        FromDate = DateTime.Now,
                        ToDate = null,
                        Cost = 10000,
                    }
                };

                ApprenticeshipDetails = _fixture.Build<Apprenticeship>()
                   .With(s => s.Id, 1)
                   .With(s => s.Cohort, cohort)
                   .With(s => s.PaymentStatus, PaymentStatus.Completed)
                   .With(s => s.EndDate, DateTime.UtcNow)
                   .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
                   .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                   .With(s => s.PriceHistory, priceHistory)
                   .Without(s => s.ApprenticeshipUpdate)
                   .Without(s => s.DataLockStatus)
                   .Without(s => s.EpaOrg)
                   .Without(s => s.Continuation)
                   .Without(s => s.PreviousApprenticeship)
                   .Without(s => s.EmailAddressConfirmed)
                   .Without(s => s.ApprenticeshipConfirmationStatus)
                   .Without(s => s.OverlappingTrainingDateRequests)
                   .Create();

                Db.Apprenticeships.Add(ApprenticeshipDetails);

                var existingApprenticeshipDetails = _fixture.Build<DraftApprenticeship>()
                 .With(s => s.Id, 2)
                 .With(s => s.Cohort, cohort1)
                 .With(s => s.EndDate, DateTime.UtcNow)
                 .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                 .Without(s => s.ApprenticeshipUpdate)
                 .Without(s => s.EpaOrg)
                 .Without(s => s.PreviousApprenticeship)
                 .Without(s => s.EmailAddressConfirmed)
                 .Without(s => s.ApprenticeshipConfirmationStatus)
                 .Without(s => s.OverlappingTrainingDateRequests)
                 .Create();

                Db.DraftApprenticeships.Add(existingApprenticeshipDetails);

                OverlappingTrainingDateRequest = new OverlappingTrainingDateRequest()
                    .Set(s => s.Id, 1)
                    .Set(s => s.DraftApprenticeshipId, existingApprenticeshipDetails.Id)
                    .Set(s => s.PreviousApprenticeshipId, ApprenticeshipDetails.Id)
                    .Set(s => s.Status, OverlappingTrainingDateRequestStatus.Pending);

                Db.OverlappingTrainingDateRequests.Add(OverlappingTrainingDateRequest);

                Db.SaveChanges();
            }

            internal async Task ResolveApprenticeshipByStoppingApprenticeship()
            {
                await _sut.Resolve(ApprenticeshipDetails.Id, OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped);
            }

            internal async Task ResolveApprenticeshipByUpdatingStopDate()
            {
                await _sut.Resolve(ApprenticeshipDetails.Id, OverlappingTrainingDateRequestResolutionType.StopDateUpdate);
            }

            internal async Task ResolveApprenticeshipByApprenticeshipUpdate()
            {
                await _sut.Resolve(ApprenticeshipDetails.Id, OverlappingTrainingDateRequestResolutionType.ApprenticeshipUpdate);
            }

            internal void VerifyOverlappingServiceIsCalled()
            {
                _overlapCheckService.Verify(x => x.CheckForOverlapsOnStartDate(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, It.IsAny<CancellationToken>()), Times.Once);
            }

            internal void VerifyOverlappingServiceIsNotCalled()
            {
                _overlapCheckService.Verify(x => x.CheckForOverlapsOnStartDate(It.IsAny<string>(), It.IsAny<CommitmentsV2.Domain.Entities.DateRange>(), null, It.IsAny<CancellationToken>()), Times.Never);
            }
        }
    }
}
