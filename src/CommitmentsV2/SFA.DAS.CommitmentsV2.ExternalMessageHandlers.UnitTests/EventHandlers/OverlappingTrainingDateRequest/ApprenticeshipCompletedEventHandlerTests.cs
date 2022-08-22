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
    public class ApprenticeshipCompletedEventHandlerTests
    {
        public ApprenticeshipCompletedEventHandlerFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new ApprenticeshipCompletedEventHandlerFixture();
        }

        [Test]
        public async Task Handle_WhenHandlingCompletionEventIsReceived_ThenItCallsResolveOverlapService()
        {
            await _fixture.Handle();
            _fixture.VerifyCallsResolveOverlappingTrainingDateRequestService();
        }

        public class ApprenticeshipCompletedEventHandlerFixture
        {
            private ApprenticeshipCompletedEventHandler _handler;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private ApprenticeshipCompletedEvent _event;
            private FakeLogger<ApprenticeshipCompletedEventHandler> _logger;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;

            public ApprenticeshipCompletedEventHandlerFixture()
            {
                var autoFixture = new Fixture();
                _logger = new FakeLogger<ApprenticeshipCompletedEventHandler>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _handler = new ApprenticeshipCompletedEventHandler(_logger, _resolveOverlappingTrainingDateRequestService.Object);
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _event = autoFixture.Create<ApprenticeshipCompletedEvent>();
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
