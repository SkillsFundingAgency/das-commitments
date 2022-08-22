using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.ExternalHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Fakes;

namespace SFA.DAS.CommitmentsV2.ExternalMessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    public class ApprenticeshipCompletionDateUpdatedEventHandlerTests
    {
        public ApprenticeshipCompletionDateUpdatedEventHandlerTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipCompletionDateUpdatedEventHandlerTestsFixture();
        }
        [Test]
        public async Task Handle_WhenHandlingCompletionEventIsReceived_ThenItCallsResolveOverlapService()
        {
            await _fixture.Handle();
            _fixture.VerifyCallsResolveOverlappingTrainingDateRequestService();
        }

        public class ApprenticeshipCompletionDateUpdatedEventHandlerTestsFixture
        {
            private ApprenticeshipCompletionDateUpdatedEventHandler _handler;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private ApprenticeshipCompletionDateUpdatedEvent _event;
            private FakeLogger<ApprenticeshipCompletionDateUpdatedEvent> _logger;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;

            public ApprenticeshipCompletionDateUpdatedEventHandlerTestsFixture()
            {
                var autoFixture = new Fixture();
                _logger = new FakeLogger<ApprenticeshipCompletionDateUpdatedEvent>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _handler = new ApprenticeshipCompletionDateUpdatedEventHandler(_logger, _resolveOverlappingTrainingDateRequestService.Object);
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _event = autoFixture.Create<ApprenticeshipCompletionDateUpdatedEvent>();
            }

            public async Task Handle()
            {
                await _handler.Handle(_event, _messageHandlerContext.Object);
            }

            internal void VerifyCallsResolveOverlappingTrainingDateRequestService()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_event.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.CompletionDateEvent), Times.Once);
            }
        }
    }
}
