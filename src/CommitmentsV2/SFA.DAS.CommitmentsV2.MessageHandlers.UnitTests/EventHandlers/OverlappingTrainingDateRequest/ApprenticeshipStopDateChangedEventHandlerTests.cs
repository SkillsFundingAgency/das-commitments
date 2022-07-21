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
    public class ApprenticeshipStopDateChangedEventHandlerTests
    {
        [Test]
        public async Task Handle_ApprenticeshipStopDateChangedEvent_ThenShouldResolvePendingOverlappingTrainingDateRequest()
        {
            var fixture = new ApprenticeshipStopDateChangedEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.Verify_OverlappingTrainingDateRequest_Resolved();
        }

        private class ApprenticeshipStopDateChangedEventHandlerTestsFixture
        {
            private ApprenticeshipStopDateChangedEvent _apprenticeshipStopDateChangedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private ApprenticeshipStopDateChangedEventHandler _sut;

            public ApprenticeshipStopDateChangedEventHandlerTestsFixture()
            {
                _apprenticeshipStopDateChangedEvent = new ApprenticeshipStopDateChangedEvent()
                {
                    ApprenticeshipId = 1
                };
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new ApprenticeshipStopDateChangedEventHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public async Task Handle()
            {
                await _sut.Handle(_apprenticeshipStopDateChangedEvent, _messageHandlerContext.Object);
            }

            public void Verify_OverlappingTrainingDateRequest_Resolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_apprenticeshipStopDateChangedEvent.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.StopDateUpdate), Times.Once);
            }
        }
    }
}
