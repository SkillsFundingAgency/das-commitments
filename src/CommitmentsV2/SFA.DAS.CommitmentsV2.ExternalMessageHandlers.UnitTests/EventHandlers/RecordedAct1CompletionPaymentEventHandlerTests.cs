using System;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Payments.ProviderPayments.Messages;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.ExternalHandlers.UnitTests.EventHandlers
{
    [TestFixture]
    public class RecordedAct1CompletionPaymentEventHandlerTests
    {
        public RecordedAct1CompletionPaymentEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new RecordedAct1CompletionPaymentEventHandlerTestsFixture();
        }

        [TestCase(ApprenticeshipStatus.Live)]
        [TestCase(ApprenticeshipStatus.Paused)]
        [TestCase(ApprenticeshipStatus.Stopped)]
        public async Task When_HandlingCompletionEvent_CompletionIsCalled(ApprenticeshipStatus status)
        {
            _fixture.WithApprenticeshipStatus(status);
            await _fixture.Handle();
            _fixture.VerifyApprenticeCompleteWasCalled();
            _fixture.VerifyHasInfo();
        }

        [Test]
        public async Task When_HandlingCompletionEventWithCompletedStatus_UpdateCompletionDateIsCalled()
        {
            _fixture.WithApprenticeshipStatus(ApprenticeshipStatus.Completed);
            await _fixture.Handle();
            _fixture.VerifyApprenticeUpdateCompletionDateWasCalled();
            _fixture.VerifyHasInfo();
        }

        [TestCase(ApprenticeshipStatus.Unknown)]
        [TestCase(ApprenticeshipStatus.WaitingToStart)]
        public async Task When_HandlingCompletionEventWithIncorrectStatus_WarningMessageIsLogged(ApprenticeshipStatus status)
        {
            _fixture.WithApprenticeshipStatus(status);
            await _fixture.Handle();
            _fixture.VerifyHasWarning();
        }

        [Test]
        public void Handle_WhenHandlingCompletionEventAndItFails_ThenItShouldThrowAnExceptionAndLogIt()
        {
            Assert.ThrowsAsync<NullReferenceException>(() => _fixture.SetupNullMessage().Handle());
            _fixture.VerifyHasError();
        }

        [Test]
        public async Task Handle_WhenHandlingCompletionEventAndItHasNoApprenticeshipId_ThenItLogAWarning()
        {
            _fixture.SetApprenticeshipIdToNull();
            await _fixture.Handle();
            _fixture.VerifyHasWarning();
        }

        [Test]
        public async Task Handle_WhenHandlingCompletionEventIsReceived_ThenItCallsResolveOverlapService()
        {
            _fixture.WithApprenticeshipStatus(ApprenticeshipStatus.WaitingToStart);
            await _fixture.Handle();
            _fixture.VerifyCallsResolveOverlappingTrainingDateRequestService();
        }

        public class RecordedAct1CompletionPaymentEventHandlerTestsFixture
        {
            private RecordedAct1CompletionPaymentEventHandler _handler;
            private RecordedAct1CompletionPayment _event;
            public Mock<ProviderCommitmentsDbContext> _dbContext { get; set; }
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private FakeLogger<RecordedAct1CompletionPaymentEventHandler> _logger;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private FakeApprenticeship _apprenticeship;
            private Cohort _cohort;

            public RecordedAct1CompletionPaymentEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();

                _dbContext = new Mock<ProviderCommitmentsDbContext>(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options) { CallBase = true };
                _logger = new FakeLogger<RecordedAct1CompletionPaymentEventHandler>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();

                _handler = new RecordedAct1CompletionPaymentEventHandler(new Lazy<ProviderCommitmentsDbContext>(() => _dbContext.Object), _logger);

                _messageHandlerContext = new Mock<IMessageHandlerContext>();

                _event = autoFixture.Create<RecordedAct1CompletionPayment>();

                _cohort = new Cohort() { Id = 1 };

                _apprenticeship = new FakeApprenticeship { Id = _event.ApprenticeshipId.Value, CommitmentId = 1 };
                _dbContext.Object.Apprenticeships.Add(_apprenticeship);

                _cohort.Apprenticeships.Add(_apprenticeship);
                _dbContext.Object.Cohorts.Add(_cohort);
                _dbContext.Object.SaveChanges();
            }

            public RecordedAct1CompletionPaymentEventHandlerTestsFixture WithApprenticeshipStatus(ApprenticeshipStatus status)
            {
                switch (status)
                {
                    case ApprenticeshipStatus.Live:
                        _apprenticeship.PaymentStatus = PaymentStatus.Active;
                        _apprenticeship.StartDate = _event.EventTime.UtcDateTime.AddMonths(-6);
                        break;
                    case ApprenticeshipStatus.WaitingToStart:
                        _apprenticeship.PaymentStatus = PaymentStatus.Active;
                        _apprenticeship.StartDate = _event.EventTime.UtcDateTime.AddMonths(6);
                        break;
                    case ApprenticeshipStatus.Completed:
                        _apprenticeship.PaymentStatus = PaymentStatus.Completed;
                        break;
                    case ApprenticeshipStatus.Paused:
                        _apprenticeship.PaymentStatus = PaymentStatus.Paused;
                        break;
                    case ApprenticeshipStatus.Stopped:
                        _apprenticeship.PaymentStatus = PaymentStatus.Withdrawn;
                        break;
                }

                return this;
            }

            public RecordedAct1CompletionPaymentEventHandlerTestsFixture SetupNullMessage()
            {
                _event = null;
                return this;
            }

            public RecordedAct1CompletionPaymentEventHandlerTestsFixture SetApprenticeshipIdToNull()
            {
                _event.ApprenticeshipId = null;
                return this;
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            public void VerifyApprenticeCompleteWasCalled()
            {
                Assert.AreEqual(_apprenticeship.ValuePassedToComplete, _event.EventTime.UtcDateTime);
            }

            public void VerifyApprenticeUpdateCompletionDateWasCalled()
            {
                Assert.AreEqual(_apprenticeship.ValuePassedToUpdateCompletionDate, _event.EventTime.UtcDateTime);
            }

            public void VerifyHasError()
            {
                Assert.IsTrue(_logger.HasErrors);
            }

            public void VerifyHasWarning()
            {
                Assert.IsTrue(_logger.HasWarnings);
            }

            public void VerifyHasInfo()
            {
                Assert.IsTrue(_logger.HasInfo);
            }

            internal void VerifyCallsResolveOverlappingTrainingDateRequestService()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_event.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.CompletionDateEvent), Times.Once);
            }
        }

        private class FakeApprenticeship : Apprenticeship
        {
            public PaymentStatus TestStatus { get; set; }
            public DateTime ValuePassedToComplete { get; set; }
            public DateTime ValuePassedToUpdateCompletionDate { get; set; }

            public override void Complete(DateTime completionDate)
            {
                ValuePassedToComplete = completionDate;
            }

            public override void UpdateCompletionDate(DateTime completionDate)
            {
                ValuePassedToUpdateCompletionDate = completionDate;
            }
        }
    }
}
