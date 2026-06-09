using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Exceptions;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;
using SFA.DAS.UnitOfWork.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DateRange = SFA.DAS.CommitmentsV2.Domain.Entities.DateRange;
using IMessageSession = NServiceBus.IMessageSession;

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

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeship_StopDateAndCodeAreUpdated()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            var stopDate = DateTime.Today.AddMonths(-1);
            _fixture.SetEventValues(apprentice.Id, new DateTime(stopDate.Year, stopDate.Month, 1), 12);

            await _fixture.Handle();
            _fixture.VerifyStopDateIsAssignedCorrectly();
            _fixture.VerifyWithdrawnReasonCodeIsAssignedCorrectly();
            _fixture.VerifyApprenticeshipStopEventIsCorrectlyPublished();
            _fixture.VerifyApprenticeshipStopDateChangedEventIsNotPublished();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToAlreadyWithdrawnApprenticeshipWithSameStopDate_OnlyStopEventPublished()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Withdrawn);
            var stopDate = DateTime.Today.AddMonths(-1);
            stopDate = new DateTime(stopDate.Year, stopDate.Month, 1);
            apprentice.StopDate = stopDate;
            _fixture.SetEventValues(apprentice.Id, stopDate, 12);

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipStopEventIsCorrectlyPublished();
            _fixture.VerifyApprenticeshipStopDateChangedEventIsNotPublished();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToAlreadyWithdrawnApprenticeshipWithNewStopDate_OnlyStopDateChangedEventPublished()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Withdrawn);
            var stopDate = DateTime.Today.AddMonths(-2);
            stopDate = new DateTime(stopDate.Year, stopDate.Month, 1);
            apprentice.StopDate = stopDate.AddMonths(1);
            _fixture.SetEventValues(apprentice.Id, stopDate, 12);

            await _fixture.Handle();
            _fixture.VerifyApprenticeshipStopEventIsNotPublished();
            _fixture.VerifyApprenticeshipStopDateChangedEventIsCorrectlyPublished();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeshipWithRedundancyReasonCode_RedundancyFlagIsSet()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            apprentice.MadeRedundant = null;
            var stopDate = DateTime.Today.AddMonths(-1);
            _fixture.SetEventValues(apprentice.Id, new DateTime(stopDate.Year, stopDate.Month, 1), Apprenticeship.WithdrawalReasonCode_MadeRedundant);

            await _fixture.Handle();
            apprentice.MadeRedundant.Should().BeTrue();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeshipWithoutRedundancyReason_RedundancyFlagIsNotSet()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            apprentice.MadeRedundant = null;
            var stopDate = DateTime.Today.AddMonths(-1);
            _fixture.SetEventValues(apprentice.Id, new DateTime(stopDate.Year, stopDate.Month, 1), 12);

            await _fixture.Handle();
            apprentice.MadeRedundant.Should().BeFalse();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedToExistingApprenticeship_StoreLearnerHistoryCommand_IsPublished()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            var stopDate = DateTime.Today.AddMonths(-1);
            _fixture.SetEventValues(apprentice.Id, new DateTime(stopDate.Year, stopDate.Month, 1), 12);

            await _fixture.Handle();
            _fixture.VerifyStoreLearnerHistoryCommandIsSent();
        }

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedOnNonApprenticeship_Exception_IsThrown()
        {
            var func = () => _fixture.Handle();

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

        [Test]
        public async Task When_LearnerWithDrawnEvent_AppliedOnOverlappingOLTD_Exception_IsThrown()
        {
            var apprentice = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            var newstopDate = DateTime.Today.AddMonths(-1);
            _fixture.SetWithdrawnDateEvent(new DateTime(newstopDate.Year, newstopDate.Month, 1));
            _fixture.SetOverlapCheckStatusToFailForThisApprenticeship(apprentice);

            // Act
            var exception = Assert.ThrowsAsync<DomainException>(_fixture.Handle);

            // Assert
            exception.DomainErrors.Should().ContainEquivalentOf(new { PropertyName = "stopDate", ErrorMessage = "The date overlaps with existing dates for the same apprentice" });
        }

        [Test]
        public async Task Handle_LearnerWithDrawnEvent_ThenShouldResolveDataLocks()
        {
            // Arrange
            var apprenticeship = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            _fixture.SetWithdrawnDateEvent(stopDate);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyDataLocksAreResolvedCorrectly();
        }

        [Test]
        public async Task Handle_LearnerWithDrawnEvent_ThenResolveOltd()
        {
            // Arrange
            var apprenticeship = await _fixture.SetupApprenticeship(PaymentStatus.Active);
            var stopDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            _fixture.SetWithdrawnDateEvent(stopDate);

            // Act
            await _fixture.Handle();

            // Assert
            _fixture.VerifyOltdIsCalledCorrectly();
        }

        public class LearnerWithdrawnEventHandlerTestsFixture
        {
            private LearnerWithdrawnEventHandler _handler;
            private LearnerWithdrawnEvent _event;
            private ProviderCommitmentsDbContext _dbContext { get; set; }
            private Mock<ICurrentDateTime> _currentDateTime { get; set; }
            private Mock<IOverlapCheckService> _overlapCheckService { get; set; }
            private UnitOfWorkContext _unitOfWorkContext { get; set; }
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOLTDRequestService { get; set; }
            private Mock<IMessageSession> _messageSession;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private FakeLogger<LearnerWithdrawnEventHandler> _logger;

            public LearnerWithdrawnEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                            .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                                            .Options);
                _unitOfWorkContext = new UnitOfWorkContext();
                _currentDateTime = new Mock<ICurrentDateTime>();
                _overlapCheckService = new Mock<IOverlapCheckService>();
                _overlapCheckService.Setup(x => x.CheckForOverlaps(It.IsAny<string>(), It.IsAny<DateRange>(), It.IsAny<long?>(), It.IsAny<CancellationToken>())).ReturnsAsync(new OverlapCheckResult(false, false));
                _resolveOLTDRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _messageSession = new Mock<IMessageSession>();

                _logger = new FakeLogger<LearnerWithdrawnEventHandler>();

                _handler = new LearnerWithdrawnEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext), _currentDateTime.Object,
                    _overlapCheckService.Object, _resolveOLTDRequestService.Object, _messageSession.Object, _logger);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _event = autoFixture.Create<LearnerWithdrawnEvent>();
            }

            public LearnerWithdrawnEventHandlerTestsFixture SetEventValues(long apprenticeshipId, DateTime withdrawnDate, int withdrawnReasoncode)
            {
                _event.ApprenticeshipId = apprenticeshipId;
                _event.WithdrawnDate = withdrawnDate;
                _event.WithdrawnReasonCode = withdrawnReasoncode;
                return this;
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

            public LearnerWithdrawnEventHandlerTestsFixture SetOverlapCheckStatusToFailForThisApprenticeship(Apprenticeship apprenticeship)
            {
                _overlapCheckService.Setup(x => x.CheckForOverlaps(apprenticeship.Uln, It.IsAny<DateRange>(), apprenticeship.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new OverlapCheckResult(true, true));
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            public void VerifyStopDateIsAssignedCorrectly()
            {
                var apprenticeship = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
                apprenticeship.StopDate.Should().Be(_event.WithdrawnDate);
            }

            public void VerifyApprenticeshipStopEventIsCorrectlyPublished()
            {
                var apprenticeship = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
                var stoppedEvent = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipStoppedEvent>().FirstOrDefault();
                stoppedEvent.Should().NotBeNull();
                stoppedEvent.AppliedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
                stoppedEvent.ApprenticeshipId.Should().Be(_event.ApprenticeshipId);
                stoppedEvent.StopDate.Should().Be(_event.WithdrawnDate);
                stoppedEvent.IsWithDrawnAtStartOfCourse.Should().Be(apprenticeship.StartDate.Value == _event.WithdrawnDate);
                stoppedEvent.LearnerDataId.Should().Be(apprenticeship.LearnerDataId);
                stoppedEvent.ProviderId.Should().Be(apprenticeship.Cohort.ProviderId);
                stoppedEvent.IsWithdrawnViaIlr.Should().BeTrue();
            }

            public void VerifyApprenticeshipStopEventIsNotPublished()
            {
                var stoppedEvent = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipStoppedEvent>().FirstOrDefault();
                stoppedEvent.Should().BeNull();
            }

            public void VerifyApprenticeshipStopDateChangedEventIsCorrectlyPublished()
            {
                var apprenticeship = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
                var stoppedEvent = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipStopDateChangedEvent>().First();
                stoppedEvent.Should().NotBeNull();
                stoppedEvent.ChangedOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
                stoppedEvent.ApprenticeshipId.Should().Be(_event.ApprenticeshipId);
                stoppedEvent.StopDate.Should().Be(_event.WithdrawnDate);
                stoppedEvent.IsWithDrawnAtStartOfCourse.Should().Be(apprenticeship.StartDate.Value == _event.WithdrawnDate);
                stoppedEvent.LearnerDataId.Should().Be(apprenticeship.LearnerDataId);
                stoppedEvent.ProviderId.Should().Be(apprenticeship.Cohort.ProviderId);
                stoppedEvent.IsWithdrawnViaIlr.Should().BeTrue();
            }

            public void VerifyApprenticeshipStopDateChangedEventIsNotPublished()
            {
                var stoppedEvent = _unitOfWorkContext.GetEvents().OfType<ApprenticeshipStopDateChangedEvent>().FirstOrDefault();
                stoppedEvent.Should().BeNull();
            }


            public void VerifyWithdrawnReasonCodeIsAssignedCorrectly()
            {
                var apprenticeship = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
                apprenticeship.WithdrawnReasonCode.Should().Be(_event.WithdrawnReasonCode);
            }

            public void VerifyStoreLearnerHistoryCommandIsSent()
            {
                _messageSession.Verify(x => x.Send(It.Is<StoreLearningHistoryCommand>(c =>
                    c.ApprenticeshipId == _event.ApprenticeshipId &&
                    c.Source == Types.LearningSourceType.ILRStatusChange &&
                    c.ChangeType == Types.LearningChangeType.AutoApproved &&
                    c.LearningKey == _event.LearningKey &&
                    c.AppliedDate == _event.Created &&
                    c.Description == $"ILR Learner status changed from Live to Withdrawn due to {_event.WithdrawnReasonCode}"
                ), It.IsAny<SendOptions>()), Times.Once);
            }

            public void VerifyDataLocksAreResolvedCorrectly()
            {
                var apprenticeship = _dbContext.Apprenticeships.Find(_event.ApprenticeshipId);
                var dataLockAssertion = _dbContext.DataLocks.Where(s => s.ApprenticeshipId == apprenticeship.Id).ToList();
                dataLockAssertion.Should().HaveCount(4);
                dataLockAssertion.Where(s => s.IsResolved).Should().HaveCount(2);
            }

            public void VerifyOltdIsCalledCorrectly()
            {
                _resolveOLTDRequestService
                    .Verify(x => x.Resolve(_event.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.StopDateUpdate), Times.Once);
            }

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
                    StartDate = startDate != null ? startDate.Value : DateTime.UtcNow.AddMonths(-2),
                    Uln = "1234567890"
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
    }
}
