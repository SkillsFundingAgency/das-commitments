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
    internal class DraftApprenticeshipDeletedEventHandlerTests
    {
        [Test]
        public async Task Handle_DraftApprenticeshipDeletedEvent_ThenShouldResolvePendingOverlappingTrainingDateRequest()
        {
            var fixture = new DraftApprenticeshipDeletedEventHandlerTestsFixture();
            await fixture.Handle();
            fixture.Verify_OverlappingTrainingDateRequest_Resolved();
        }

        private class DraftApprenticeshipDeletedEventHandlerTestsFixture
        {
            private DraftApprenticeshipDeletedEvent _draftApprenticeshipDeletedEvent;
            private Mock<IMessageHandlerContext> _messageHandlerContext;
            private Mock<IResolveOverlappingTrainingDateRequestService> _resolveOverlappingTrainingDateRequestService;
            private DraftApprenticeshipDeletedEventHandler _sut;

            public DraftApprenticeshipDeletedEventHandlerTestsFixture()
            {
                _draftApprenticeshipDeletedEvent = new DraftApprenticeshipDeletedEvent() { DraftApprenticeshipId = 1, CohortId = 1};
                
                _messageHandlerContext = new Mock<IMessageHandlerContext>();
                _resolveOverlappingTrainingDateRequestService = new Mock<IResolveOverlappingTrainingDateRequestService>();
                _sut = new DraftApprenticeshipDeletedEventHandler(_resolveOverlappingTrainingDateRequestService.Object);
            }

            public async Task Handle()
            {
                await _sut.Handle(_draftApprenticeshipDeletedEvent, _messageHandlerContext.Object);
            }

            public void Verify_OverlappingTrainingDateRequest_Resolved()
            {
                _resolveOverlappingTrainingDateRequestService.Verify(x => x.DraftApprenticeshpDeleted(_draftApprenticeshipDeletedEvent.DraftApprenticeshipId, Types.OverlappingTrainingDateRequestResolutionType.DraftApprentieshipDeleted), Times.Once);
            }
        }
    }
}
