using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class LearnerWithdrawnEventHandlerTests
    {
        public LearnerWithdrawnEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new LearnerWithdrawnEventHandlerTestsFixture();
        }

        //[Test]
        //public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeship_StopDateAndCodeAreUpdated()
        //{
        //    await _fixture.Handle();
        //    _fixture.VerifyStopDateIsAssignedCorrectly();
        //    _fixture.VerifyWithdrawnReasonCodeIsAssignedCorrectly();
        //}

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedOnNonApprenticeship_Exception_IsThrown()
        {
            var func = () => _fixture.Handle();

            //_fixture.SetApprenticeshipPaymentStatus(PaymentStatus.Completed);
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Completed);
            _fixture.SetApprenticeshipIdOnEvent(apprentice.Id + 1);

            // Act
            await func.Should().ThrowAsync<BadRequestException>();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedOnCompletedApprenticeship_Exception_IsThrown()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Completed);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(_fixture.Handle);

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = "Apprenticeship cannot be Stopped if Payment Status is Completed. Unable to stop apprenticeship" });
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedWithStopDatePriorToStartOfFutureCourse_Exception_IsThrown()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active, DateTime.Today.AddMonths(2));

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(_fixture.Handle);

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = "Invalid stop date. Date should be value of start date if training has not started." });
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedWithFutureStopDate_Exception_IsThrown()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            _fixture.SetWithdrawnDateEvent(DateTime.Today.AddMonths(1));

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(_fixture.Handle);

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = "Invalid Stop Date. Stop date cannot be in the future and must be the 1st of the month." });
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedWithStopDatePriorToStartOfCourse_Exception_IsThrown()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            _fixture.SetWithdrawnDateEvent(DateTime.Today.AddMonths(-4));

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(_fixture.Handle);

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = "Invalid Stop Date. Stop date cannot be before the apprenticeship has started." });
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedWithStopDateWhichIsNot1stOfMonth_Exception_IsThrown()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            var newstopDate = DateTime.Today.AddMonths(-1);
            _fixture.SetWithdrawnDateEvent(new DateTime(newstopDate.Year, newstopDate.Month, 15));

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(_fixture.Handle);

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = "Invalid Stop Date. Stop date must be the 1st of the month." });
        }

        public class LearnerWithdrawnEventHandlerTestsFixture
        {
            private LearnerWithdrawnEventHandler _handler;
            private LearnerWithdrawnEvent _event;
            public ProviderCommitmentsDbContext _dbContext { get; set; }
            public Mock<ICurrentDateTime> _currentDateTime { get; set; }
            public Mock<IOverlapCheckService> _overlapCheckService { get; set; }
            public Mock<IResolveOverlappingTrainingDateRequestService> _resolveOLTDRequestService { get; set; }
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private FakeLogger<LearnerWithdrawnEventHandler> _logger;
            //private FakeApprenticeship _apprenticeship;
            //private Cohort _cohort;

            public LearnerWithdrawnEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);

                _currentDateTime = new Mock<ICurrentDateTime>();
                _overlapCheckService = new Mock<IOverlapCheckService>();
                _resolveOLTDRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();

                _logger = new FakeLogger<LearnerWithdrawnEventHandler>();

                _handler = new LearnerWithdrawnEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _currentDateTime.Object,
                    _overlapCheckService.Object, _resolveOLTDRequestService.Object, _logger);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _event = autoFixture.Create<LearnerWithdrawnEvent>();
            }

            public LearnerWithdrawnEventHandlerTestsFixture SetApprenticeshipIdOnEvent(long id)
            {
                _event.ApprenticeshipId = id;
                return this;
            }

            public LearnerWithdrawnEventHandlerTestsFixture SetWithdrawnDateEvent(DateTime date)
            {
                _event.WithdrawnDate = date;
                return this;
            }

            //public LearnerWithdrawnEventHandlerTestsFixture SetApprenticeshipPaymentStatus(PaymentStatus paymentStatus)
            //{
            //    _apprenticeship.PaymentStatus = paymentStatus;
            //    _dbContext.SaveChanges();
            //    return this;
            //}


            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            //public void VerifyStopDateIsAssignedCorrectly()
            //{
            //    _apprenticeship.StopDate.Should().Be(_event.WithdrawnDate);
            //}

            //public void VerifyWithdrawnReasonCodeIsAssignedCorrectly()
            //{
            //    _apprenticeship.WithdrawnReasonCode.Should().Be(_event.WithdrawnReasonCode);
            //}

            //public void VerifyHasError()
            //{
            //    Assert.That(_logger.HasErrors, Is.True);
            //}

            public async Task<Apprenticeship> SetupApprenticeship(PaymentStatus paymentStatus = PaymentStatus.Active, DateTime? startDate = null)
            {
                var today = DateTime.UtcNow;
                _currentDateTime.Setup(a => a.UtcNow).Returns(today);

                var fixture = new Fixture();
                var apprenticeshipId = _event.ApprenticeshipId;
                var apprenticeship = new Apprenticeship
                {
                    Id = apprenticeshipId,
                    Cohort = new Cohort
                    {
                        EmployerAccountId = fixture.Create<long>(),
                        AccountLegalEntity = new AccountLegalEntity()
                    },
                    DataLockStatus = SetupDataLocks(apprenticeshipId),
                    PaymentStatus = paymentStatus,
                    StartDate = startDate != null ? startDate.Value : DateTime.UtcNow.AddMonths(-2)
                };

                _dbContext.Apprenticeships.Add(apprenticeship);
                await _dbContext.SaveChangesAsync();

                return apprenticeship;
            }

            private static ICollection<DataLockStatus> SetupDataLocks(long apprenticeshipId)
            {
                var activeDataLock4 = new DataLockStatus
                {
                    ApprenticeshipId = apprenticeshipId,
                    EventStatus = EventStatus.New,
                    IsExpired = false,
                    TriageStatus = TriageStatus.Restart,
                    ErrorCode = DataLockErrorCode.Dlock04
                };

                var activeDataLock5 = new DataLockStatus
                {
                    ApprenticeshipId = apprenticeshipId,
                    EventStatus = EventStatus.New,
                    IsExpired = false,
                    TriageStatus = TriageStatus.Restart,
                    ErrorCode = DataLockErrorCode.Dlock05
                };

                var inactiveDataLock6 = new DataLockStatus
                {
                    ApprenticeshipId = apprenticeshipId,
                    EventStatus = EventStatus.Removed,
                    IsExpired = false,
                    TriageStatus = TriageStatus.Restart,
                    ErrorCode = DataLockErrorCode.Dlock04
                };

                var dataLockForApprenticeshipBeforeStart = new DataLockStatus
                {
                    ApprenticeshipId = apprenticeshipId,
                    EventStatus = EventStatus.New,
                    IsExpired = false,
                    TriageStatus = TriageStatus.Change,
                    ErrorCode = DataLockErrorCode.Dlock04
                };

                return new List<DataLockStatus> { activeDataLock4, activeDataLock5, inactiveDataLock6, dataLockForApprenticeshipBeforeStart };
            }

        }

        private class FakeApprenticeship : Apprenticeship
        {
        }
    }
}
