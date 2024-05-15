using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    [Parallelizable]
    public class ApprenticeshipUpdatedApprovedEventHandlerTests
    {
        [Test]
        public async Task Handle_ApprenticeshipStoppedEvent_ThenShouldResolvePendingOverlappingTrainingDateRequest()
        {
            var fixture = new ApprenticeshipUpdatedApprovedEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.Verify_OverlappingTrainingDateRequest_Resolved();
        }

        private class ApprenticeshipUpdatedApprovedEventHandlerTestsFixture
        {
            private ApprenticeshipUpdatedApprovedEvent _apprenticeshipUpdateApprovedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private ApprenticeshipUpdatedApprovedEventHandler _sut;

            public ApprenticeshipUpdatedApprovedEventHandlerTestsFixture()
            {
                _apprenticeshipUpdateApprovedEvent = new ApprenticeshipUpdatedApprovedEvent()
                {
                    ApprenticeshipId = 1
                };
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new ApprenticeshipUpdatedApprovedEventHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public async Task Handle()
            {
                await _sut.Handle(_apprenticeshipUpdateApprovedEvent, _messageHandlerContext.Object);
            }

            public void Verify_OverlappingTrainingDateRequest_Resolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(_apprenticeshipUpdateApprovedEvent.ApprenticeshipId,null, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipUpdate), Times.Once);
            }
        }
    }
}
