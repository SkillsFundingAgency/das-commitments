using System.Threading.Tasks;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipStopDateChangedEventHandlerTests
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
            private ApprenticeshipStopDateChangedEvent _apprenticeshipStoppedDateChangedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private ApprenticeshipStopDateChangedEventHandler _sut;

            public ApprenticeshipStopDateChangedEventHandlerTestsFixture()
            {
                _apprenticeshipStoppedDateChangedEvent = new ApprenticeshipStopDateChangedEvent()
                {
                    ApprenticeshipId = 1
                };
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new ApprenticeshipStopDateChangedEventHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public async Task Handle()
            {
                await _sut.Handle(_apprenticeshipStoppedDateChangedEvent, _messageHandlerContext.Object);
            }

            public void Verify_OverlappingTrainingDateRequest_Resolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_apprenticeshipStoppedDateChangedEvent.ApprenticeshipId,null, Types.OverlappingTrainingDateRequestResolutionType.StopDateUpdate), Times.Once);
            }
        }
    }
}
