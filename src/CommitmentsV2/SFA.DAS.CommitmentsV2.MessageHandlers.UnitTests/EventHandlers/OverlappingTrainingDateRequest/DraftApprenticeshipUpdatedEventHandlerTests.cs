using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers.OverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Messages.Events;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.EventHandlers.OverlappingTrainingDateRequest
{
    [TestFixture]
    [Parallelizable]
    internal class DraftApprenticeshipUpdatedEventHandlerTests
    {
        [Test]
        public async Task Handle_ApprenticeshipStoppedEvent_ThenShouldResolvePendingOverlappingTrainingDateRequest()
        {
            var fixture = new DraftApprenticeshipUpdatedEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.Verify_OverlappingTrainingDateRequest_Resolved();
        }

        private class DraftApprenticeshipUpdatedEventHandlerTestsFixture
        {
            private DraftApprenticeshipUpdatedEvent _draftApprenticeshipUpdatedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private DraftApprenticeshipUpdatedEventHandler _sut;

            public DraftApprenticeshipUpdatedEventHandlerTestsFixture()
            {
                _draftApprenticeshipUpdatedEvent = new DraftApprenticeshipUpdatedEvent(1, 1, "XXXXX", Guid.NewGuid(), DateTime.UtcNow);
                
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new DraftApprenticeshipUpdatedEventHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public async Task Handle()
            {
                await _sut.Handle(_draftApprenticeshipUpdatedEvent, _messageHandlerContext.Object);
            }

            public void Verify_OverlappingTrainingDateRequest_Resolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.Resolve(null, _draftApprenticeshipUpdatedEvent.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.DraftApprenticeshipUpdated), Times.Once);
            }
        }
    }
}
