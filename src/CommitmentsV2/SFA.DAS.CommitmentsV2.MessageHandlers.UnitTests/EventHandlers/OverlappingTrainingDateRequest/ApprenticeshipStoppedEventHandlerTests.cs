using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipStoppedEventHandlerTests
    {
        [Test]
        public async Task Handle_ApprenticeshipStoppedEvent_ThenShouldResolvePendingOverlappingTrainingDateRequest()
        {
            var fixture = new ApprenticeshipStopDateChangedEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.Verify_OverlappingTrainingDateRequest_Resolved();
        }

        private class ApprenticeshipStopDateChangedEventHandlerTestsFixture
        {
            private ApprenticeshipStoppedEvent _apprenticeshipStoppedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private ApprenticeshipStoppedEventHandler _sut;

            public ApprenticeshipStopDateChangedEventHandlerTestsFixture()
            {
                _apprenticeshipStoppedEvent = new ApprenticeshipStoppedEvent()
                {
                    ApprenticeshipId = 1
                };
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new ApprenticeshipStoppedEventHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public async Task Handle()
            {
                await _sut.Handle(_apprenticeshipStoppedEvent, _messageHandlerContext.Object);
            }

            public void Verify_OverlappingTrainingDateRequest_Resolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.ResolveByApprenticeship(_apprenticeshipStoppedEvent.ApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped), Times.Once);
            }
        }
    }
}
